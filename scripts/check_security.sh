#!/bin/bash

echo "=================================================="
echo "          SSH SECURITY & AUTO-BAN REPORT"
echo "          Date: $(date)"
echo "=================================================="

AUTH_LOG="/var/log/auth.log"
FAILED_ATTEMPT_THRESHOLD=10
BOTNET_THRESHOLD=3

# Internal whitelist of default safe IPs
SAFE_IPS="127.0.0.1 ::1"
ENV_FILE="/home/ubuntu/app/.env"

# Read AllowedIps from .env file if it exists, replacing commas with spaces and stripping CIDR notation
if [ -f "$ENV_FILE" ]; then
    ENV_IPS=$(grep "^AllowedIps=" "$ENV_FILE" | cut -d '=' -f2 | tr ',' ' ' | sed 's/\/[0-9]*//g')
    SAFE_IPS="$SAFE_IPS $ENV_IPS"
fi

if [ ! -f "$AUTH_LOG" ]; then
    echo "Auth log not found: $AUTH_LOG"
    exit 1
fi

# Detect potential botnets by /24 subnet activity
echo "[*] Scanning for potential botnetworks..."

# STRICT FILTER: Pre-filter log for failures before extracting IPs for subnet calculation
BOTNET_SUBNETS=$(sudo grep -E "Failed password|Invalid user" "$AUTH_LOG" \
    | grep -oE '\b([0-9]{1,3}\.){3}[0-9]{1,3}\b' \
    | cut -d. -f1-3 \
    | sort \
    | uniq -c \
    | awk -v threshold="$BOTNET_THRESHOLD" '$1 > threshold {print $2}')

for subnet in $BOTNET_SUBNETS; do
    echo "Potential botnet detected in subnet: ${subnet}.0/24"

    # STRICT FILTER: Ensure we only ban IPs from this subnet that actually caused errors
    IP_LIST=$(sudo grep -E "Failed password|Invalid user" "$AUTH_LOG" \
        | grep "${subnet}\." \
        | grep -oE '\b([0-9]{1,3}\.){3}[0-9]{1,3}\b' \
        | sort -u)

    for ip in $IP_LIST; do
        # Whitelist check: do not ban developer IPs
        if [[ " $SAFE_IPS " =~ " $ip " ]]; then
            echo "Skipping developer/safe IP: $ip"
            continue
        fi
        
        echo "Banning botnet member: $ip"
        sudo fail2ban-client set sshd banip "$ip" >/dev/null 2>&1
    done
done

# Ban IPs with excessive failed login attempts
echo
echo "[*] Checking for IPs with more than ${FAILED_ATTEMPT_THRESHOLD} failed attempts..."

AGGRESSIVE_IPS=$(sudo grep "Failed password" "$AUTH_LOG" \
    | grep -oE '\b([0-9]{1,3}\.){3}[0-9]{1,3}\b' \
    | sort \
    | uniq -c \
    | sort -nr)

while read -r count ip; do
    if [ -n "$ip" ] && [ "$count" -gt "$FAILED_ATTEMPT_THRESHOLD" ]; then
        # Whitelist check: do not ban developer IPs
        if [[ " $SAFE_IPS " =~ " $ip " ]]; then
            echo "Skipping developer/safe IP: $ip"
            continue
        fi
        
        echo "Banning $ip (Attempts: $count)"
        sudo fail2ban-client set sshd banip "$ip" >/dev/null 2>&1
    fi
done <<< "$AGGRESSIVE_IPS"

echo
echo "[1] TOP 10 ATTACKING IPs"

sudo grep "Failed password" "$AUTH_LOG" \
    | grep -oE '\b([0-9]{1,3}\.){3}[0-9]{1,3}\b' \
    | sort \
    | uniq -c \
    | sort -nr \
    | head -n 10

echo
echo "[2] TOP 10 TARGETED USERNAMES"

sudo grep "Invalid user" "$AUTH_LOG" \
    | awk '{print $8}' \
    | sort \
    | uniq -c \
    | sort -nr \
    | head -n 10

echo
echo "[3] FAIL2BAN SSHD STATUS"

sudo fail2ban-client status sshd \
    | grep -E "Currently banned|Total banned|Banned IP list"

echo "=================================================="