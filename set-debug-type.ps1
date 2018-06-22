$projectNames = @("Invio.Extensions.Linq", "Invio.Extensions.Linq.Async")

foreach ($projectName in $projectNames) {
  copy "src\${projectName}\${projectName}.csproj" "src\${projectName}\${projectName}.csproj.bak"

  $project = New-Object XML
  $project.Load("${pwd}\src\${projectName}\${projectName}.csproj")
  $project.Project.PropertyGroup[1].DebugType = "full"
  $project.Save("${pwd}\src\${projectName}\${projectName}.csproj")
}
