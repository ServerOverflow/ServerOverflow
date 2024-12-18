from bs4 import BeautifulSoup
import requests
import json

response = requests.get("https://wiki.vg/index.php?title=Protocol_version_numbers")

if response.status_code != 200:
    print("Failed to retrieve the page")
    exit(1)

html = BeautifulSoup(response.text, features="html.parser")
row_span = 0
data = {}

table = html.find('table').find('tbody')
if not table:
    print("Failed to find table's body")
    exit(1)

for tr in table.find_all("tr"):
    td = tr.find_all("td")
    if len(td) != 3:
        continue
    name = td[0].text.strip()
    if row_span == 0:
        if "rowspan" in td[1]:
            row_span = int(td[1]["rowspan"]) - 1
        else:
            row_span = 1
        text = td[1].text
        proto = int(text.split(" ")[1]) + 0x40000000 if text.startswith("Snapshot") else int(text)
        data[proto] = name
    row_span -= 1

with open("ServerOverflow/Resources/protocol.json", "w") as f:
    json.dump(data, f)

print(f"Successfully processed {len(data)} entries")
