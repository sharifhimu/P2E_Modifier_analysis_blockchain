import csv
import json
import logging
import os
import time
from web3 import Web3

# ========================
# CONFIGURATION
# ========================

# Ganache / Local Node RPC URL
RPC_URL = "http://192.168.100.60:8545"

# Your deployed contract address
CONTRACT_ADDRESS = "0xC19757601A06392DA12a21CC640f27E04Df4753a"

# ABI file path
ABI_PATH = "TokenPair.json"

# The event name in your contract
EVENT_NAME = "SentimentUpdated"

# CSV file to store event data
CSV_FILE = "events_log.csv"

# ========================
# LOGGING SETUP
# ========================
logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s %(levelname)s: %(message)s"
)

# ========================
# CONNECT TO NODE
# ========================
w3 = Web3(Web3.HTTPProvider(RPC_URL))

if not w3.is_connected():
    logging.error(f"Could not connect to provider: {RPC_URL}")
    exit(1)

logging.info(f"Connected to provider: {RPC_URL}")

# ========================
# LOAD ABI
# ========================
if not os.path.exists(ABI_PATH):
    logging.error(f"ABI file not found: {ABI_PATH}")
    exit(1)

with open(ABI_PATH, "r") as abi_file:
    abi_json = json.load(abi_file)
    # If it's a dict with "abi" key, extract just the ABI array
    if isinstance(abi_json, dict) and "abi" in abi_json:
        contract_abi = abi_json["abi"]
    else:
        contract_abi = abi_json

# ========================
# CONTRACT INSTANCE
# ========================
contract_address_checksum = w3.to_checksum_address(CONTRACT_ADDRESS)
contract = w3.eth.contract(address=contract_address_checksum, abi=contract_abi)

# ========================
# CREATE EVENT FILTER
# ========================
try:
    event_filter = contract.events[EVENT_NAME].create_filter(from_block="latest")
except Exception as e:
    logging.error(f"Could not create event filter for '{EVENT_NAME}': {e}")
    exit(1)

logging.info(f"Listening for event: {EVENT_NAME}")

# ========================
# WRITE HEADER IF CSV EMPTY
# ========================
if not os.path.exists(CSV_FILE):
    with open(CSV_FILE, mode="w", newline="", encoding="utf-8") as file:
        writer = csv.writer(file)
        writer.writerow(["blockNumber", "transactionHash", "args"])

# ========================
# EVENT LOOP
# ========================
while True:
    try:
        for event in event_filter.get_new_entries():
            logging.info(f"New event: {event}")
            
            block_number = event.blockNumber
            tx_hash = event.transactionHash.hex()
            args = dict(event.args)

            # Save to CSV
            with open(CSV_FILE, mode="a", newline="", encoding="utf-8") as file:
                writer = csv.writer(file)
                writer.writerow([block_number, tx_hash, json.dumps(args)])

        time.sleep(2)

    except Exception as e:
        logging.error(f"Error while listening: {e}")
        time.sleep(5)
