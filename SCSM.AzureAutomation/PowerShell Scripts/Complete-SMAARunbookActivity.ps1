# base script still needs to be update for the workflow to use
$ID = ""
$SMClass = Get-SCSMClass -name SCSM.WorkItem.AzureAutomationWebhookActivity$
$SMObj = Get-SCSMObject -Class $SMClass -Filter "ID -eq $ID"
$Params = @{
Status = $Status
}
Set-SCSMObject -SMObject $SMObj -propertyhashtable $Params