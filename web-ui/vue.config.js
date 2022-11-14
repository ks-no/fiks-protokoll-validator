const { defineConfig } = require("@vue/cli-service");
module.exports = defineConfig({
    lintOnSave: false,
    configureWebpack: {
        resolve: {
            fallback: {
                path: require.resolve("path-browserify"),
                os: false,
                fs: false
            },
            alias: {
                "vue": "@vue/compat"
            }
        },
    },
  publicPath: "/fiks-validator/",
  devServer: {
      proxy: {
          "^/api": {
              target: "https://localhost:44303",
              ws: true,
              changeOrigin: true
          }
      }
  },
});
