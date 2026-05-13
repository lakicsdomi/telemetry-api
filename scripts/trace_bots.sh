#!/bin/bash
# trace_bots.sh - List unique IPs and their origins

echo "--- RECENT TRAFFIC LOG ---"
# Extract unique IPs from X-Forwarded-For logs
IPS=$(sudo docker compose logs server | grep "X-Forwarded-For" | awk '{print $NF}' | sort -u)

for ip in $IPS; do
    echo "Investigating: $ip"
    # Use ip-api.com (Free for 45 requests/min)
    curl -s "http://ip-api.com/json/$ip?fields=status,message,country,city,isp,as,mobile,proxy" | jq .
    echo "--------------------------"
    sleep 1.5 # To avoid hitting rate limits
done