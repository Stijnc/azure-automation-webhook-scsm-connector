$JobID = ""
$secpasswd = ConvertTo-SecureString $passwd -AsPlainText -Force
$cred = New-Object System.Management.Automation.PSCredential ($AAAcount, $secpasswd) 
Add-AzureAccount -credential $cred
Switch-AzureMode AzureResourceManager
$Job = Get-AzureAutomationJob -Id $JobID -ResourceGroupName $ResourcegroupName -AutomationAccountName $AutomationAccountName
