def sdk = resolveDotNetSDKToolVersion("6.0")

pipeline {
    options {
        buildDiscarder(logRotator(numToKeepStr: '50', artifactNumToKeepStr: '50'))
        disableConcurrentBuilds()
        timeout(time: 60, unit: 'MINUTES')
        timestamps ()
    }
    agent any
    environment {
        PROJECT_WEB_FOLDER = "web-ui"
        PROJECT_API_FOLDER = "api"
        PROJECT_TEST = "KS.FiksProtokollValidator.Tests/KS.FiksProtokollValidator.Tests.csproj"
        PROJECT_CHARTNAME = "fiks-protokoll-validator"
        API_APP_NAME = "fiks-protokoll-validator-api"
        WEB_APP_NAME = "fiks-protokoll-validator-web"
        DOCKERFILE_TESTS = "Dockerfile-run-tests"
        ARTIFACTORY_CREDENTIALS = "artifactory-token-based"
        DOCKER_REPO_RELEASE = "https://docker-all.artifactory.fiks.ks.no"
        DOCKER_REPO = "https://docker-local-snapshots.artifactory.fiks.ks.no"
        DOTNET_CLI_HOME = "/tmp/DOTNET_CLI_HOME"
        TMPDIR = "${env.PWD + '\\tmpdir'}"
        BUILD_OPTS = buildOpts(env.VERSION_SUFFIX)
    }
    parameters {
        booleanParam(defaultValue: false, description: 'Skal prosjektet releases?', name: 'isRelease')
        string(name: "specifiedVersion", defaultValue: "", description: "Hva er det nye versjonsnummeret (X.X.X)? Som default releases snapshot-versjonen")
        text(name: "releaseNotes", defaultValue: "Ingen endringer utført", description: "Hva er endret i denne releasen?")
        text(name: "securityReview", defaultValue: "Endringene har ingen sikkerhetskonsekvenser", description: "Har endringene sikkerhetsmessige konsekvenser, og hvilke tiltak er i så fall iverksatt?")
        string(name: "reviewer", defaultValue: "Endringene krever ikke review", description: "Hvem har gjort review?")
    }
    stages {
        stage('Initialize') {
            steps {
                script {
                    env.GIT_SHA = sh(returnStdout: true, script: 'git rev-parse HEAD').substring(0, 7)
                    env.REPO_NAME = scm.getUserRemoteConfigs()[0].getUrl().tokenize('/').last().split("\\.")[0]
                    env.CURRENT_VERSION = findVersionPrefix()
                    env.NEXT_VERSION = params.specifiedVersion == "" ? incrementVersion(env.CURRENT_VERSION) : params.specifiedVersion
                    if(params.isRelease) {
                      env.VERSION_SUFFIX = ""
                      env.BUILD_SUFFIX = ""
                      env.FULL_VERSION = env.CURRENT_VERSION
                    } else {
                      def timestamp = getTimestamp()
                      env.VERSION_SUFFIX = "build.${timestamp}"
                      env.BUILD_SUFFIX = "--version-suffix ${env.VERSION_SUFFIX}"
                      env.FULL_VERSION = "${CURRENT_VERSION}-${env.VERSION_SUFFIX}"
                    }
                }
            }
        }
        stage('Build docker images') {
            parallel {      
                stage('API: Build and publish docker image') {
                    agent {
                      label 'linux || linux-large'
                    }
                    tools {
                      dotnetsdk sdk
                    }
                    environment {
                      NUGET_HTTP_CACHE_PATH = "${env.WORKSPACE + '@tmp/cache'}"
                      TMPDIR = "${env.PWD}/tmpdir"
                      MSBUILDDEBUGPATH = "${env.TMPDIR}"
                      NUGET_CONF = credentials('nuget-config')
                      DOTNET_CLI_TELEMETRY_OPTOUT = 1
                      COMPlus_EnableDiagnostics = 0
                      DOTNET_GCHeapHardLimit=20000000
                    }
                    steps {
                        withDotNet(sdk: sdk) {
                            dir("api\\KS.FiksProtokollValidator.WebAPI") {      
                              dotnetRestore(
                                configfile: NUGET_CONF,
                                showSdkInfo: true,
                                verbosity: 'normal'
                              )
                              dotnetPublish(
                                configuration: 'Release',
                                nologo: true,
                                noRestore: true,
                                optionsString: env.BUILD_OPTS,
                                outputDirectory: 'published-api'
                              )
                              script {
                                println("API: Building and publishing docker image version: ${env.FULL_VERSION}")
                                buildAndPushDockerImage(API_APP_NAME, [env.FULL_VERSION, 'latest'], [], params.isRelease, ".")
                              }  
                            }
                        }
                    }
                    post {
                        success {
                            recordIssues enabledForFailure: true, tools: [msBuild()]
                        }
                    }
                }
                stage('WEB: Build and publish docker image') {
                    steps {
                        script {
                            println("WEB: Building and publishing docker image version: ${env.FULL_VERSION}")
                            buildAndPushDockerImageWeb(params.isRelease);
                        }
                    }
                }
            }
        }
        stage('API and WEB: Push helm chart') {
            steps {
                println("API and WEB: Building helm chart version: ${env.FULL_VERSION}")
                buildHelmChart(PROJECT_CHARTNAME, env.FULL_VERSION)
            }
        }
        
        stage('API og WEB - Snapshot: Set version') {
            when {
                expression { !params.isRelease }
            }
            steps {
               script {
                   env.IMAGE_TAG = env.FULL_VERSION
               }
           }
        }
        
        stage('API og WEB: Deploy to dev') {
            when {
                anyOf {
                    branch 'master'
                    branch 'main'
                }
                expression { !params.isRelease }
            }
            steps {
                build job: 'deployToDev', parameters: [string(name: 'chartName', value: PROJECT_CHARTNAME), string(name: 'version', value: env.FULL_VERSION)], wait: false, propagate: false
            }
        }
        
        stage('API og WEB: Release. Set next version and push to git') {
            when {
                allOf {
                  expression { params.isRelease }
                  expression { return env.NEXT_VERSION }
                  expression { return env.FULL_VERSION }
                }
            }
            steps {
                gitCheckout("main")
                gitTag(isRelease, env.FULL_VERSION)
                prepareDotNetNoBuild(env.NEXT_VERSION)
                script {
                    currentBuild.description = "${env.user} released version ${env.FULL_VERSION}"
                }
            }
            post {
                success {
                  gitPush()
                  createGithubRelease env.REPO_NAME, params.reviewer, params.releaseNotes, env.CURRENT_VERSION, env.user
                }
            }
        }
    }
    
    post {
        always {
            dir("${PROJECT_API_FOLDER}\\bin") {
                deleteDir()
            }
            dir("${PROJECT_WEB_FOLDER}\\bin") {
                deleteDir()
            }
            dir("${PROJECT_TEST}\\bin") {
                deleteDir()
            }
            dir("${env.TMPDIR}") {
              deleteDir()
            }
            deleteDir()
        }
    }
}

def versionPattern() {
  return java.util.regex.Pattern.compile("^(\\d+)\\.(\\d+)\\.(\\d+)(.*)?")
}

def findVersionPrefix() {
  
    def files = findCsprojFiles()
    
    def versions = files.collect {
      echo("Checking ${it}")
      return extractVersion(readFile(file: it.getPath().trim(), encoding: 'UTF-8'))
    }.findAll {
      return it != null && ! it.trim().isEmpty()
    }
    echo "Found ${versions.size()} versions"
    if(versions.size() > 0 && versions[0] != "") {
      def currentVersion = versions[0]
      echo "Version: ${currentVersion}"
      return currentVersion
    } else {
      throw new Exception("No versionPrefix fond in csproj files")
    }
}

def findCsprojFiles() {
  def files = findFiles(glob: '**/*.csproj').findAll {
    return it.getName().toUpperCase().contains("TEST") == false && it.getName().toUpperCase().contains("EXAMPLE") == false
  }
  if(files.size() == 0) {
    throw new Exception("No csproj files found")
  }
  echo("Found ${files.size()} csproj files")
  return files
}

@NonCPS
def extractVersion(xml) {
  def debom = { data ->
    if(data?.length() > 0 && data[0] == '\uFEFF') return data.drop(1) else return data
  }
  def xmlData = debom.call(xml)
  def Project = new XmlSlurper().parseText(xmlData)
  def version = Project['PropertyGroup']['VersionPrefix'].text().trim()
  echo("Found version ${version}")
  return version
}

def incrementVersion(versionString) {
    def p = versionPattern()
    def m = p.matcher(versionString)
    if(m.find()) {
        def major = m.group(1) as Integer
        def minor = m.group(2) as Integer
        def patch = m.group(3) as Integer
        return "${major}.${minor}.${++patch}"
    } else {
        return null
    }
}

def buildOpts(versionSuffix) {
  if(versionSuffix == null || versionSuffix.trim().isEmpty()) {
    echo("No build opts will be passed to dotnet build")
    return ""
  } else {
    def opts = "--version-suffix ${versionSuffix}"
    echo("Will add build opts: ${opts}")
    return opts
  }
}

def getTimestamp() {
    return java.time.OffsetDateTime.now().format(java.time.format.DateTimeFormatter.ofPattern("yyyyMMddHHmmssSSS"))
}

def buildAndPushDockerImageApi(boolean isRelease = false) {
  def repo = isRelease ? DOCKER_REPO_RELEASE : DOCKER_REPO
  dir("api") {
    script {
      def customImage
    
      println("Building API code in Docker image")
      
      docker.image('docker-all.artifactory.fiks.ks.no/dotnet/sdk:6.0').inside('-e DOTNET_CLI_HOME=/tmp -e XDG_DATA_HOME=/tmp') {
        sh '''
            dotnet publish --configuration Release KS.FiksProtokollValidator.WebAPI/KS.FiksProtokollValidator.WebAPI.csproj --output published-api
        '''
      }
      
      println("Building API image")
      customImage = docker.build("${API_APP_NAME}:${FULL_VERSION}", ".")
      
      docker.withRegistry(repo, ARTIFACTORY_CREDENTIALS)
      {
        println("Publishing API image")
        customImage.push()
      }
    }
  }
}

def buildAndPushDockerImageWeb(boolean isRelease = false) {
  def repo = isRelease ? DOCKER_REPO_RELEASE : DOCKER_REPO
  dir("web-ui") {
    script {
      println("Building WEB code in Docker image")
      if (isRelease) {
        repo = 'https://docker-local.artifactory.fiks.ks.no'
      } else {
        repo = 'https://docker-local-snapshots.artifactory.fiks.ks.no'
      }
      
      docker.image('node:16').inside() {
        sh '''
          npm install
        '''
        sh '''
          npm run build -- --mode production
        '''
      }
      
      docker.withRegistry(repo, 'artifactory-token-based') {
        def customImage = docker.build("${WEB_APP_NAME}:${FULL_VERSION}", ".")
        println("Publishing WEB image")
        tags.each {
          customImage.push(it)
        }
      }
    }
  }
}
