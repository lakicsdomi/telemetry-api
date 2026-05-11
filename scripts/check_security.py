#!/usr/bin/env python3

import re
import subprocess
from collections import Counter, defaultdict
from pathlib import Path
from datetime import datetime

AUTH_LOG = "/var/log/auth.log"
FAILED_ATTEMPT_THRESHOLD = 10
BOTNET_THRESHOLD = 3


def ban_ip(ip):
    subprocess.run(
        f"sudo fail2ban-client set sshd banip {ip}",
        shell=True,
        stdout=subprocess.DEVNULL,
        stderr=subprocess.DEVNULL
    )


def extract_ips(text):
    return re.findall(r"\b(?:\d{1,3}\.){3}\d{1,3}\b", text)


def get_subnet(ip):
    return ".".join(ip.split(".")[:3])


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

    subnet_map = defaultdict(set)

    for line in lines:
        ips = extract_ips(line)

        for ip in ips:
            subnet = get_subnet(ip)
            subnet_map[subnet].add(ip)

    for subnet, ips in subnet_map.items():
        if len(ips) > BOTNET_THRESHOLD:
            print(f"Potential botnet detected in subnet: {subnet}.0/24")

            for ip in sorted(ips):
                print(f"Banning botnet member: {ip}")
                ban_ip(ip)

    # Ban aggressive IPs
    print(f"\n[*] Checking for IPs with more than {FAILED_ATTEMPT_THRESHOLD} failed attempts...")

    failed_ips = []

    for line in lines:
        if "Failed password" in line:
            failed_ips.extend(extract_ips(line))

    ip_counter = Counter(failed_ips)

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

    for line in lines:
        if "Invalid user" in line:
            match = re.search(r"Invalid user (\S+)", line)
            if match:
                usernames.append(match.group(1))

    username_counter = Counter(usernames)

    for username, count in username_counter.most_common(10):
        print(f"{count:>5} {username}")

    # Fail2Ban status
    print("\n[3] FAIL2BAN SSHD STATUS")

    result = subprocess.run(
        "sudo fail2ban-client status sshd",
        shell=True,
        capture_output=True,
        text=True
    )

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