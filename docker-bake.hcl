// Build the test executable against the actual production build/runtime stages.
target "server-build" {
  context = "."
  dockerfile = "Dockerfile"
  target = "build"
  platforms = ["linux/amd64"]
}

target "server-runtime" {
  context = "."
  dockerfile = "Dockerfile"
  target = "runtime"
  platforms = ["linux/amd64"]
}

target "native-tests" {
  context = "."
  dockerfile = "Dockerfile.tests"
  target = "native-tests"
  platforms = ["linux/amd64"]
  contexts = {
    server-build = "target:server-build"
    server-runtime = "target:server-runtime"
  }
  tags = ["sprk-native-tests:ci"]
}
