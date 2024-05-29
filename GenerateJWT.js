// Base64 URL encoding function
function base64url(source) {
  // Encode in classical base64
  var encodedSource = CryptoJS.enc.Base64.stringify(source);

  // Remove padding equal characters
  encodedSource = encodedSource.replace(/=+$/, '');

  // Replace characters according to base64url specifications
  encodedSource = encodedSource.replace(/\+/g, '-');
  encodedSource = encodedSource.replace(/\//g, '_');

  return encodedSource;
}

// JWT generation function
function generateJWT(payload, secret) {
  var header = {
    "alg": "HS256",
    "typ": "JWT"
  };

  var stringifiedHeader = CryptoJS.enc.Utf8.parse(JSON.stringify(header));
  var encodedHeader = base64url(stringifiedHeader);

  var stringifiedData = CryptoJS.enc.Utf8.parse(JSON.stringify(payload));
  var encodedData = base64url(stringifiedData);

  var token = encodedHeader + "." + encodedData;

  var signature = CryptoJS.HmacSHA256(token, secret);
  signature = base64url(signature);

  var signedToken = token + "." + signature;
  return signedToken;
}

// Get the private key from a secure location
var secret = context.getVariable('privateKey'); // Ensure this is securely stored

// Get the request body
var requestBody = context.getVariable('request.content');

// Parse the request body as JSON
var requestBodyJson;
try {
  requestBodyJson = JSON.parse(requestBody);
} catch (e) {
  throw new Error('Invalid JSON format in request body');
}

// Extract userId and tokenId from the request body
var userId = requestBodyJson.userId;
var tokenId = requestBodyJson.tokenId;

// Validate inputs
if (!userId || !tokenId) {
  throw new Error('Missing required fields: userId and tokenId');
}

// Example input validation (e.g., alphanumeric, specific length)
var userIdPattern = /^[a-zA-Z0-9]{6,20}$/;
var tokenIdPattern = /^[a-zA-Z0-9]{6,20}$/;

if (!userIdPattern.test(userId) || !tokenIdPattern.test(tokenId)) {
  throw new Error('Invalid input: userId and tokenId must be alphanumeric and 6-20 characters long');
}

// Create the JWT payload
var payload = {
  userId: userId,
  tokenId: tokenId,
  exp: Math.floor(Date.now() / 1000) + (60 * 60) // 1 hour expiration
};

// Generate the JWT
var jwt = generateJWT(payload, secret);

// Set the JWT in the response context
context.setVariable('jwt', jwt);
