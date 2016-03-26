param ([string]$ConnectorID)
$TypePath = (Get-ItemProperty "hklm:\software\microsoft\system center\2010\service manager\setup").InstallDirectory + "scsm.azureautomation.wpf.dll"
Add-Type -Path $TypePath

$mpcAARunbook = Get-SCSMClass -Name SCSM.AzureAutomation.Runbook$
$mpcAAConnector = Get-SCSMClass -Name SCSM.AzureAutomation.Connector$
$emoAAConnector = Get-SCSMObject -Class $mpcAAConnector -Filter "Id -eq $ConnectorID"

$SubscriptionID = $emoAAConnector.SubscriptionID
$AutomationAccountName = $emoAAConnector.AutomationAccount
$username = $emoAAConnector.RunAsAccountName
$encryptedPassword = $emoAAConnector.RunAsAccountPassword
$secpassword = ConvertTo-SecureString([SCSM.AzureAutomation.WPF.Connector.StringCipher]::Decrypt($encryptedPassword,$username)) -AsPlainText -Force
$ResourceGroup = $emoAAConnector.ResourceGroup

$Creds = New-Object System.Management.Automation.PSCredential ($username, $secpassword)
Login-AzureRmAccount -Credential $Creds -SubscriptionId $SubscriptionID

#Get Each Runbook and Save to CMDB
$Runbooks = Get-AzureRmAutomationRunbook -ResourceGroupName $ResourceGroup -AutomationAccountName $AutomationAccountName 
Foreach($runbook in $Runbooks) {
 $Runbookobj = Get-AzureRmAutomationRunbook -AutomationAccountName $AutomationAccountName -ResourceGroup $ResourceGroup -Name $runbook.Name
 if ($Runbookobj.State -ne "New")
	{
		$RBType = switch($Runbookobj.RunbookType)
		{
			Script{"AzureAutomationRunbook.Type.Workflow"}
			Graph{"AzureAutomationRunbook.Type.Graphical"}
			PowerShell{"AzureAutomationRunbook.Type.PowerShell"}
		}

		$param = @()
        ($Runbookobj.Parameters).GetEnumerator() | Foreach-object { 
            $object = [PSObject]@{
                Parameter = $_.Name
                DefaultValue = ($_.Value).DefaultValue
                IsMandatory = ($_.Value).IsMandatory
                Position = ($_.Value).Position  
                Type = ($_.Value).Type 
                }
            $param += $object
        }
        $JsonParam = $param | ConvertTo-Json

		$RunbookHT = @{
			ConnectorID = $ConnectorID
            DisplayName = $Runbookobj.Name
			Name = $Runbookobj.Name
			Description = $Runbookobj.Description
			LastModifiedtime = $Runbookobj.LastModifiedTime
			LogVerbose =$Runbookobj.LogVerbose
			LogProgress = $Runbookobj.LogProgress
			CreatedDate = $Runbookobj.CreationTime
			RunbookType = $RBType
			Status = "AzureAutomationRunbook.Status.Published"
			Parameters = $JsonParam.ToString()
		}

  $rbName = $Runbookobj.Name
  $Runbook = Get-SCSMObject -Class $mpcAARunbook -Filter "Name -eq $rbName"
  if($Runbook -eq $null) {
   New-SCSMObject -Class $mpcAARunbook -PropertyHashtable $RunbookHT
  }
        else {
            Set-SCSMObject -SMObject $Runbook -PropertyHashtable $RunbookHT
        }
 }
}