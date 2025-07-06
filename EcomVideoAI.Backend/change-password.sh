#!/bin/bash

# Script to change password on DigitalOcean droplet
set -e

echo "🔐 Changing root password on DigitalOcean droplet"
echo "================================================="

DROPLET_IP="209.38.121.252"
OLD_PASSWORD="c4f9495dd734a243dd9f56cc57"
NEW_PASSWORD="Promax@9000"

echo "Connecting to droplet to change password..."

# Use expect to handle password change
expect << EOF
set timeout 30

spawn ssh -o StrictHostKeyChecking=no root@$DROPLET_IP

expect {
    "Current password:" {
        send "$OLD_PASSWORD\r"
        expect "New password:"
        send "$NEW_PASSWORD\r"
        expect "Retype new password:"
        send "$NEW_PASSWORD\r"
        expect "password updated successfully"
        send "exit\r"
    }
    timeout {
        puts "Timeout occurred"
        exit 1
    }
}

expect eof
EOF

echo "✅ Password changed successfully!"
echo "New password: $NEW_PASSWORD"
echo ""
echo "Now you can deploy using the new password." 