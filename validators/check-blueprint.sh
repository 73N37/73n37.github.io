#!/bin/bash
echo "===================================================="
echo "Checking Source C# Blueprint Alignment..."
echo "===================================================="

SRC_DIR="AIDA-M365/src"

if [ ! -d "$SRC_DIR" ]; then
    echo "[ERROR] Source directory $SRC_DIR does not exist."
    exit 1
fi

REQUIRED_FILES=(
    "$SRC_DIR/Program.cs"
    "$SRC_DIR/Components/KanbanBoard.razor"
    "$SRC_DIR/Components/Connect.razor"
    "$SRC_DIR/Components/UpcomingEvents.razor"
)

failed=0
for file in "${REQUIRED_FILES[@]}"; do
    if [ -f "$file" ]; then
        echo "[SUCCESS] Verified file blueprint: $file"
    else
        echo "[ERROR] Missing critical file blueprint: $file"
        failed=1
    fi
done

if [ $failed -eq 1 ]; then
    echo "[ERROR] Blueprint check failed."
    exit 1
else
    echo "[SUCCESS] Blueprint alignment checks complete."
    exit 0
fi
