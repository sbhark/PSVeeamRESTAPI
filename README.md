# veeampsrestapi
JSON based RESTful API that uses Veeam's PowerShell application layer to execute various commandlet across multiple Veeam Backup and Replication Servers.

# Supported Veeam Backup and Replication Versions
Production Ready:
Veeam 9.0.X
Veeam 9.5 (Limited Cmdlets)

In Development:
Veeam 9.5.X

# Requirements
.Net Framework 4.5 or greater

Internet Information Services (IIS) 8.X

Powershell / Remote Powershell
https://helpcenter.veeam.com/backup/powershell/powershell_remoting.html

Web Deploy 3.5

# Recommended Environment
Windows Server 2012 R2 with Latest Updates

Internet Information Services (IIS) 8.X

.Net Framework 4.6.1

Web Deploy 3.5

# Software Depenendencies
NLog v4.3.7

Newtonsoft.Json v9.0.1

Newtonsoft.Json.Schema v2.0.4

# Deployment Guide 
1. Create an IIS site on your server.
2. Update following configurations Web.Config
  1. devAuthorization
  2. prodAuthorization
  3. devPsProxies
  4. prodPsProxies
  5. isDev
  6. nlog
3. Deploy from Visual Studio, create your own customized publish profile to connect to your IIS Server.


