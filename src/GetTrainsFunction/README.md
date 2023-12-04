## Create Cert for DEBUGGING
Auto cert generation is currently not working on the .NET Core build.
On Windows you can run:

PS> $cert = New-SelfSignedCertificate -Subject localhost -DnsName localhost -FriendlyName "Functions Development" -KeyUsage DigitalSignature -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.1")
PS> Export-PfxCertificate -Cert $cert -FilePath certificate.pfx -Password (ConvertTo-SecureString -String <password> -Force -AsPlainText)

For more checkout https://docs.microsoft.com/en-us/aspnet/core/security/https
