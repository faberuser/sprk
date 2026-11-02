// Share production dependencies/runtime without compiling the release binary.
target "server-test-deps" {
  context = "."
  dockerfile = "Dockerfile"
  target = "test-deps"
  platforms = ["linux/amd64"]
}

target "server-runtime" {
  context = "."
  dockerfile = "Dockerfile"
  target = "runtime-base"
  platforms = ["linux/amd64"]
}

target "native-tests" {
  context = "."
  dockerfile = "Dockerfile.tests"
  target = "native-tests"
  platforms = ["linux/amd64"]
  contexts = {
    server-test-deps = "target:server-test-deps"
    server-runtime = "target:server-runtime"
  }
  tags = ["sprk-native-tests:ci"]
}
