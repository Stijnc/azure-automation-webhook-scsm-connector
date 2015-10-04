#
# Start_AARunbook.ps1
#
# Start Azure Automation Runbook
Import-Module Azure
Import-Module SMLets
Switch-AzureMode AzureResourceManager

$SMClass = Get-SCSMClass -Name SCSM.AzureAutomation.Connector$
$SMObject = Get-SCSMObject -Class $SMClass 
$SubscriptionID = $SMObject.SubscriptionID
$AutomationAccountName = $SMObject.AutomationAccount
$password = $SMObject.RunAsAccountPassword
$username = $SMObject.RunAsAccountName
$ResourceGroup = $SMObject.ResourceGroup

$secpassword = ConvertTo-SecureString $password -AsPlainText -force
$Creds = New-Object System.Management.Automation.PSCredential ($username, $secpassword)
$Account = Add-AzureAccount -Credential $Creds
$Subscription = Select-AzureSubscription -SubscriptionId $SubscriptionID
$JobID = Start-AzureAutomationRunbook -Name $RunbookName -AutomationAccountName $AccountName -ResourceGroupName $ResourceGroup -Parameters $Params

$SMClass = Get-SCSMClass -Name SCSM.AzureAutomation.Runbook.Activity$ 
$SMObj = Get-SCSMObject -Class $SMClass -Filter "ID -eq $ID"
Set-SCSMObject -SMObject $SMObj -Property JobID -Value $JobID