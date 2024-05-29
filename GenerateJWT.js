// Base64 URL encoding function
function base64url(source) {
  // Parse the source as UTF-8
  var utf8Source = CryptoJS.enc.Utf8.parse(source);

  // Encode the source to Base64
  var encodedSource = CryptoJS.enc.Base64.stringify(utf8Source);

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

  // Convert header to Base64URL
  var encodedHeader = base64url(JSON.stringify(header));

  // Convert payload to Base64URL
  var encodedPayload = base64url(JSON.stringify(payload));

  var token = encodedHeader + "." + encodedPayload;

  // Sign the token
  var signature = CryptoJS.HmacSHA256(token, secret);
  var encodedSignature = base64url(signature.toString(CryptoJS.enc.Base64));

  var signedToken = token + "." + encodedSignature;
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
