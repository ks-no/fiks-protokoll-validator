module.exports = {
  publicPath: "/fiks-validator/",
  devServer: {
      proxy: {
          "^/api": {
              target: "https://localhost:44303",
              ws: true,
              changeOrigin: true
          }
      }
  }
};
