
# TODO - replace with yq over compose.yml images
$images=@(
  'node-exporter:latest',
  'prometheus:v2.28.1',
  'grafana:8.0.5',
  'jaeger:1.24'
)

foreach ($image in $images)
{    
    docker manifest create --amend "courselabs/$($image)" `
      "courselabs/$($image)-linux-arm64" `
      "courselabs/$($image)-linux-amd64"
    
    docker manifest push "courselabs/$($image)"
}
