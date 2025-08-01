# AmcacheParser

**Amcache Filter Viewer** is a simple yet powerful console application that parses and displays Amcache entries from Windows in a formatted table view.  
It allows users to apply filters based on criteria like file name, size, SHA1 hash, or version, and highlights matching entries in red while pushing them to the top of the list.

## How to Download?
Access <a href="/releases/tag/Releases">Releases Page</a> click on AmcacheParser.exe and download file.
</br>
![](https://github.com/kahzgbb/DDetect/blob/main/step1.png?raw=true)
</br>
![](https://github.com/kahzgbb/DDetect/blob/main/step2.png?raw=true)
</br>

## 🧩 What is Amcache?

Amcache.hve is a Windows registry hive that stores information about executed programs on the system, including:
- File name and path
- File size
- SHA1 hash
- Execution time
- File version
- And more

This tool provides an easy way to visualize and filter that data.

---

## 🎯 Features

- ✅ Parses Amcache entries automatically.
- ✅ Supports filters via external `.txt` files.
- ✅ Highlights matched rows in **red**.
- ✅ Moves matched entries to the **top** for visibility.
- ✅ Supports multiple values per filter (e.g., `Size=1234,5678`).
- ✅ Clean and readable console output using `DataGridView`.

---

## 📦 Filter Format

Filter files are plain `.txt` files with one filter per line.  
You can specify filters using the following keys:

- `Name=...` — match by file name (partial or full).
- `Size=...` — match by file size(s). Multiple sizes can be comma-separated.
- `SHA1=...` — match by SHA1 hash.
- `Version=...` — match by file version string.

### 📝 Example `filters.txt`

```txt
Name=malicious.exe
Size=4796416,92672
SHA1=1A2B3C4D5E6F...
Version=1.0.0.0
