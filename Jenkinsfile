pipeline {
    agent any
    environment {
        PROJECT_WEB_FOLDER = "web-ui"
        PROJECT_API_FOLDER = "api"
        PROJECT_TEST = "KS.FiksProtokollValidator.Tests/KS.FiksProtokollValidator.Tests.csproj"
        PROJECT_CHARTNAME = "fiks-protokoll-validator"
        API_APP_NAME = "fiks-protokoll-validator-api"
        WEB_APP_NAME = "fiks-protokoll-validator-web"
        DOCKERFILE_TESTS = "Dockerfile-run-tests"
        // Artifactory credentials is stored under this key
        ARTIFACTORY_CREDENTIALS = "artifactory-token-based"
        // URL to artifactory Docker release repo
        DOCKER_REPO_RELEASE = "https://docker-all.artifactory.fiks.ks.no"
        // URL to artifactory Docker Snapshot repo
        DOCKER_REPO = "https://docker-local-snapshots.artifactory.fiks.ks.no"
        DOTNET_CLI_HOME = "/tmp/DOTNET_CLI_HOME"
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
                    env.CURRENT_VERSION = findVersionSuffix()
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
                    print("Listing all environment variables:")
                    sh 'printenv'
                }
            }
        }
              
        stage('API: Build and publish docker image') {
            steps {
                script {
                    println("API: Building and publishing docker image version: ${env.FULL_VERSION}")
                    buildAndPushDockerImageApi(params.isRelease);
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
                gitPush()
                script {
                    currentBuild.description = "${env.user} released version ${env.FULL_VERSION}"
                }
                withCredentials([usernamePassword(credentialsId: 'Github-token-login', passwordVariable: 'GITHUB_KEY', usernameVariable: 'USERNAME')]) {
                    sh "~/.local/bin/http --ignore-stdin -a ${USERNAME}:${GITHUB_KEY} POST https://api.github.com/repos/ks-no/${env.REPO_NAME}/releases tag_name=\"${env.FULL_VERSION}\" body=\"Release utført av ${env.user}\n\n## Endringer:\n${params.releaseNotes}\n\n ## Sikkerhetsvurdering: \n${params.securityReview} \n\n ## Review: \n${params.reviewer == 'Endringene krever ikke review' ? params.reviewer : "Review gjort av ${params.reviewer}"}\""
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
        }
    }
}

def versionPattern() {
  return java.util.regex.Pattern.compile("^(\\d+)\\.(\\d+)\\.(\\d+)(.*)?")
}

def findVersionSuffix() {
    println("FindVersionSuffix")
    def findCommand = $/find api/KS.FiksProtokollValidator.WebAPI -name "KS.FiksProtokollValidator.WebAPI.csproj" -exec xpath '{}' '/Project/PropertyGroup/VersionPrefix/text()' \;/$

    def version = sh(script: findCommand, returnStdout: true, label: 'Lookup current version from csproj files').trim().split('\n').find {
        return it.trim().matches(versionPattern())
    }
    println("Version found: ${version}")
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

def getTimestamp() {
    return java.time.OffsetDateTime.now().format(java.time.format.DateTimeFormatter.ofPattern("yyyyMMddHHmmssSSS"))
}

def buildAndPushDockerImageApi(boolean isRelease = false) {
  def repo = isRelease ? DOCKER_REPO_RELEASE : DOCKER_REPO
  dir("api") {
    script {
      def customImage
    
      println("Building API code in Docker image")
      docker.image('mcr.microsoft.com/dotnet/sdk:6.0.401-alpine3.16').inside() {
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
      def customImage    
     
      println("Building WEB code in Docker image")
      docker.image('node:16').inside() {
        sh '''
           npm install
        '''
        sh '''
          npm run build -- --mode production
        '''
      }
      println("Building WEB image")
      customImage = docker.build("${WEB_APP_NAME}:${FULL_VERSION}", ".")
      
      docker.withRegistry(repo, ARTIFACTORY_CREDENTIALS)
      {
        println("Publishing WEB image")
        customImage.push()
      }
    }
  }
}
