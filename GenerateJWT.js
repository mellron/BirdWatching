// JavaScript Policy - GenerateJWT.js
var jwt = require('jsonwebtoken');

var userId = context.getVariable('request.formparam.userId');
var tokenId = context.getVariable('request.formparam.tokenId');

var privateKey = 'YOUR_PRIVATE_KEY'; // Load your private key securely
var payload = {
  userId: userId,
  tokenId: tokenId
};

var token = jwt.sign(payload, privateKey, { algorithm: 'RS256', expiresIn: '1h' });

context.setVariable('jwt', token);
