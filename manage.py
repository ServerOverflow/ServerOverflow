#!/usr/bin/env python3
from requests.adapters import HTTPAdapter
from urllib3.util.retry import Retry
import subprocess
import argparse
import requests
import secrets
import shutil
import json
import time
import sys
import os

PROFILES = ["main", "snooper"]

parser = argparse.ArgumentParser(description="Manage docker compose services")
subparsers = parser.add_subparsers(dest="command")

subparsers.add_parser("start-all", help="Start all services from all profiles")
start_parser = subparsers.add_parser("start", help="Start services for a specific profile")
start_parser.add_argument("profile", choices=PROFILES, help="Service profile")

subparsers.add_parser("stop-all", help="Stop all services from all profiles")
stop_parser = subparsers.add_parser("stop", help="Stop services for a specific profile")
stop_parser.add_argument("profile", choices=PROFILES, help="Service profile")

subparsers.add_parser("restart-all", help="Restart all services from all profiles")
restart_parser = subparsers.add_parser("restart", help="Restart services for a specific profile")
restart_parser.add_argument("profile", choices=PROFILES, help="Service profile")

subparsers.add_parser("clean", help="DANGEROUS! Remove all volumes")
subparsers.add_parser("reset", help="Stop all services and remove all volumes")

logs_parser = subparsers.add_parser("logs", help="Show the logs of services for a specific profile")
logs_parser.add_argument("profile", choices=PROFILES, help="Service profile")

args = parser.parse_args()

def docker_exec(args):
    try:
        subprocess.run(
            ["docker", "compose", "version"], check=True,
            stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    except:
        print(f"ERROR: docker compose is not installed")
        exit(-1)
    subprocess.run(["docker", "compose"] + args, check=True)

def start_all():
    for profile in PROFILES:
        docker_exec(["--profile", profile, "up", "-d", "--build"])

def start(profile):
    docker_exec(["--profile", profile, "up", "-d", "--build"])

def stop_all():
    args = []
    for profile in PROFILES:
        args += ["--profile", profile]
    docker_exec(args + ["down"])

def stop(profile):
    docker_exec(["--profile", profile, "down"])

def restart_all():
    for profile in PROFILES:
        docker_exec(["--profile", profile, "restart"])

def restart(profile):
    docker_exec(["--profile", profile, "restart"])

def clean():
    shutil.rmtree("vol", ignore_errors=True)

def reset():
    stop_all()
    clean()

def logs(profile):
    docker_exec(["--profile", profile, "logs", "-f"])

commands = {
    "start-all": start_all,
    "start": lambda: start(args.profile),
    "stop-all": stop_all,
    "stop": lambda: stop(args.profile),
    "restart-all": restart_all,
    "restart": lambda: restart(args.profile),
    "clean": clean,
    "reset": reset,
    "logs": lambda: logs(args.profile)
}

commands.get(args.command, parser.print_help)()
