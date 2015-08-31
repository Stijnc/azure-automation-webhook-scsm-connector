#
# Start_AARunbook.ps1
#
# Start Azure Automation Runbook
Import-Module Azure
Import-Module SMLets
Switch-AzureMode AzureResourceManager

# "Add Code here to get password"
# "add Code to get Subscription ID"
#$Params =  "Add Code to Build hash table to pass parameters

$secpassword = ConvertTo-SecureString $password -AsPlainText -force
$Creds = New-Object System.Management.Automation.PSCredential ($username, $secpassword)
$Account = Add-AzureAccount -Credential $Creds
$Subscription = Select-AzureSubscription -SubscriptionId $SubscriptionID
$JobID = Start-AzureAutomationRunbook -Name $RunbookName -AutomationAccountName $AccountName -ResourceGroupName $ResourceGroup -Parameters $Params

$SMClass = Get-SCSMClass -Name SCSM.AzureAutomation.Runbook.Activity$ 
$SMObj = Get-SCSMObject -Class $SMClass -Filter "ID -eq $ID"
Set-SCSMObject -SMObject $SMObj -Property JobID -Value $JobID