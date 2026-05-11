#!/usr/bin/env python3

import re
import subprocess
from collections import Counter, defaultdict # For counting occurrences of IPs and usernames
from pathlib import Path
from datetime import datetime

AUTH_LOG = "/var/log/auth.log" # Path to the SSH authentication log
FAILED_ATTEMPT_THRESHOLD = 10  # Threshold for failed login attempts
BOTNET_THRESHOLD = 3           # Threshold for number of unique IPs in a subnet to consider it a potential botnet

# Internal whitelist of safe IPs
SAFE_IPS = {"127.0.0.1", "::1"}
ENV_FILE = Path("/home/ubuntu/app/.env")

# Read AllowedIps from .env file if it exists, parse CSV and strip CIDR notation
if ENV_FILE.exists():
    for line in ENV_FILE.read_text(errors="ignore").splitlines():
        if line.startswith("AllowedIps="):
            env_ips = line.split("=", 1)[1].split(",")
            for env_ip in env_ips:
                clean_ip = env_ip.split("/")[0].strip()
                SAFE_IPS.add(clean_ip)

# Ban an IP using fail2ban-client, checking the whitelist first
def ban_ip(ip):
    # Whitelist check: do not ban developer IPs
    if ip in SAFE_IPS:
        print(f"Skipping developer/safe IP: {ip}")
        return

    subprocess.run(
        f"sudo fail2ban-client set sshd banip {ip}",
        shell=True,
        stdout=subprocess.DEVNULL, # Suppress output for cleaner logs
        stderr=subprocess.DEVNULL  # Suppress errors (e.g., if IP is already banned)
    )

# Extract IP addresses from a given text using regex
def extract_ips(text):
    return re.findall(r"\b(?:\d{1,3}\.){3}\d{1,3}\b", text)

# Get the /24 subnet for a given IP address
def get_subnet(ip):
    return ".".join(ip.split(".")[:3])

# Main function to analyze auth logs and ban malicious IPs
def main():
    print("==================================================")
    print("          SSH SECURITY & AUTO-BAN REPORT")
    print(f"          Date: {datetime.now()}")
    print("==================================================")

    auth_log_path = Path(AUTH_LOG)

    if not auth_log_path.exists():
        print(f"Auth log not found: {AUTH_LOG}")
        return

    lines = auth_log_path.read_text(errors="ignore").splitlines()

    # Detect potential botnets by subnet activity
    print("[*] Scanning for potential botnetworks...")

    subnet_map = defaultdict(set) # Map of subnet to unique IPs

    for line in lines:
        # STRICT FILTER: Only process lines that indicate unauthorized access attempts
        if "Failed password" in line or "Invalid user" in line:
            ips = extract_ips(line) # Extract all IPs from the line

            for ip in ips:
                subnet = get_subnet(ip) # Get the /24 subnet
                subnet_map[subnet].add(ip) # Add the IP to the set of unique IPs for that subnet

    for subnet, ips in subnet_map.items(): # Check if the number of unique IPs in this subnet exceeds the botnet threshold
        if len(ips) > BOTNET_THRESHOLD:
            print(f"Potential botnet detected in subnet: {subnet}.0/24")

            # Ban all IPs in this subnet that have been involved in failed login attempts
            for ip in sorted(ips):
                print(f"Banning botnet member: {ip}")
                ban_ip(ip)

    # Ban aggressive IPs
    print(f"\n[*] Checking for IPs with more than {FAILED_ATTEMPT_THRESHOLD} failed attempts...")

    # Count failed login attempts per IP
    failed_ips = []

    # Extract IPs from failed login attempts
    for line in lines:
        if "Failed password" in line:
            failed_ips.extend(extract_ips(line))

    # Count occurrences of each IP and ban those that exceed the threshold
    ip_counter = Counter(failed_ips)

    # Ban IPs that exceed the failed attempt threshold
    for ip, count in ip_counter.most_common():
        if count > FAILED_ATTEMPT_THRESHOLD:
            print(f"Banning {ip} (Attempts: {count})")
            ban_ip(ip)

    # Top attacking IPs
    print("\n[1] TOP 10 ATTACKING IPs")

    for ip, count in ip_counter.most_common(10):
        print(f"{count:>5} {ip}")

    # Top targeted usernames
    print("\n[2] TOP 10 TARGETED USERNAMES")

    usernames = []

    # Extract targeted usernames from failed login attempts
    for line in lines:
        if "Invalid user" in line:
            match = re.search(r"Invalid user (\S+)", line)
            if match:
                usernames.append(match.group(1))

    username_counter = Counter(usernames)

    # Print the top 10 targeted usernames
    for username, count in username_counter.most_common(10):
        print(f"{count:>5} {username}")

    # Fail2Ban status
    print("\n[3] FAIL2BAN SSHD STATUS")

    # Get the status of the sshd jail from fail2ban-client
    result = subprocess.run(
        "sudo fail2ban-client status sshd",
        shell=True,
        capture_output=True,
        text=True
    )

    # Print relevant lines from the fail2ban status output
    for line in result.stdout.splitlines():
        if any(keyword in line for keyword in [
            "Currently banned",
            "Total banned",
            "Banned IP list"
        ]):
            print(line)

    print("==================================================")


if __name__ == "__main__":
    main()