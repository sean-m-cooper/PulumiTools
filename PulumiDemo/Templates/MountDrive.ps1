$connectTestResult = Test-NetConnection -ComputerName [StorageAccount].file.core.windows.net -Port 445
if ($connectTestResult.TcpTestSucceeded) {
    # Save the password so the drive will persist on reboot
    cmd.exe /C "cmdkey /add:`"[StorageAccount].file.core.windows.net`" /user:`"localhost\[StorageAccount]`" /pass:`"[StorageAccountKey]`""
    # Mount the drive
    New-PSDrive -Name Z -PSProvider FileSystem -Root "\\[StorageAccount].file.core.windows.net\fsbotshare" -Persist
} else {
    Write-Error -Message "Unable to reach the Azure storage account via port 445. Check to make sure your organization or ISP is not blocking port 445, or use Azure P2S VPN, Azure S2S VPN, or Express Route to tunnel SMB traffic over a different port."
}