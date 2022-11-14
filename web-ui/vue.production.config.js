const { defineConfig } = require("@vue/cli-service");
module.exports = defineConfig({
  transpileDependencies: true,
  lintOnSave: false,
  configureWebpack: {
    resolve: {
      fallback: {
        path: require.resolve("path-browserify"),
        fs: false,
        os: false
      }
    },
  },
  publicPath: "/fiks-validator/"
});
