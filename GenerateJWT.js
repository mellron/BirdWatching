var jwt = require('jsonwebtoken');

// Read private key from a secure location (e.g., KVM, environment variable)
var privateKey = context.getVariable('privateKey'); // Ensure this is securely stored

// Get the request body
var requestBody = context.getVariable('request.content');

// Parse the request body as JSON
var requestBodyJson = JSON.parse(requestBody);

// Extract userId and tokenId from the request body
var userId = requestBodyJson.userId;
var tokenId = requestBodyJson.tokenId;

// Create the JWT payload
var payload = {
  userId: userId,
  tokenId: tokenId
};

// Options for the JWT
var options = {
  algorithm: 'RS256', // Use RS256 algorithm
  expiresIn: '1h'    // Set token expiration time
};

// Sign the JWT with the private key
var token = jwt.sign(payload, privateKey, options);

// Set the JWT in the response context
context.setVariable('jwt', token);
