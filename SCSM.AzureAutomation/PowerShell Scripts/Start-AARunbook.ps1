#
# Start_AARunbook.ps1
#
# Start Azure Automation Runbook

Add-Type -Path "C:\Users\jhennen\Source\Repos\azure-automation-webhook-scsm-connector\SCSM.AzureAutomation\SCSM.AzureAutomation.WPF\bin\Debug\scsm.azureautomation.wpf.dll"

$SMClass = Get-SCSMClass -Name SCSM.AzureAutomation.Connector$
$SMObject = Get-SCSMObject -Class $SMClass 
$SubscriptionID = $SMObject.SubscriptionID
$AutomationAccountName = $SMObject.AutomationAccount
$username = $SMObject.RunAsAccountName
$encryptedPassword = $SMObject.RunAsAccountPassword
$secpassword = ConvertTo-SecureString([SCSM.AzureAutomation.WPF.Connector.StringCipher]::Decrypt($encryptedPassword,$username)) -AsPlainText -Force
$ResourceGroup = $SMObject.ResourceGroup

$Creds = New-Object System.Management.Automation.PSCredential ($username, $secpassword)
$Account = Add-AzureAccount -Credential $Creds
$Subscription = Select-AzureSubscription -SubscriptionId $SubscriptionID
$JobID = Start-AzureAutomationRunbook -Name $RunbookName -AutomationAccountName $AccountName -ResourceGroupName $ResourceGroup -Parameters $Params

$SMClass = Get-SCSMClass -Name SCSM.AzureAutomation.Runbook.Activity$ 
$SMObj = Get-SCSMObject -Class $SMClass -Filter "ID -eq $ID"
Set-SCSMObject -SMObject $SMObj -Property JobID -Value $JobID