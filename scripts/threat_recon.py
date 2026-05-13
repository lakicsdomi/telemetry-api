#!/usr/bin/env python3
import subprocess
import time
import requests
import json

def check_ip(ip):
    # Free geolocation and proxy check API
    url = f"http://ip-api.com/json/{ip}?fields=status,country,city,isp,proxy,hosting,query"
    try:
        response = requests.get(url, timeout=5).json()
        return response
    except:
        return {"status": "fail", "query": ip}

def get_logs():
    # Extract unique IPs from Docker logs
    cmd = "sudo docker compose logs server | grep 'X-Forwarded-For' | awk '{print $NF}' | sort -u"
    try:
        ips = subprocess.check_output(cmd, shell=True).decode().split()
    except:
        return []
    
    report = []
    for ip in ips:
        if ip.startswith("172.") or ip.startswith("::ffff:172."): 
            continue # Skip internal Docker bridge IPs
        
        info = check_ip(ip)
        # Mark as suspicious if it's a proxy, VPN or hosting provider (datacenter)
        is_suspicious = info.get('proxy') or info.get('hosting')
        
        report.append({
            "IP": ip,
            "Location": f"{info.get('city')}, {info.get('country')}",
            "ISP": info.get('isp'),
            "Type": "SUSPICIOUS / BOT" if is_suspicious else "RESIDENTIAL / USER",
            "Details": info
        })
        time.sleep(1.5) # To avoid hitting API rate limits
    return report

if __name__ == "__main__":
    print("--- GENERATING THREAT REPORT ---")
    data = get_logs()
    print(json.dumps(data, indent=2))