const PROXY_CONFIG = [
  {
    context: [
      '/devstoreaccount1' 
    ],
    target: 'http://localhost:10000', 
    secure: false,
    changeOrigin: true,
    logLevel: 'debug',
    pathRewrite: { '^/devstoreaccount1': '/devstoreaccount1' }
  }
];

module.exports = PROXY_CONFIG;
