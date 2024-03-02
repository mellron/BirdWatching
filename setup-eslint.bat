@echo off
REM Install eslint
npm install eslint --save-dev

REM Install eslint-plugin-react
REM npm install eslint-plugin-react --save-dev

REM Initialize eslint
npx eslint --init

REM Install eslint-config-prettier
npm install --save-dev eslint-config-prettier

REM Install prettier with exact version
npm install --save-dev --save-exact prettier

REM Create .prettierrc file with empty JSON object
echo {} > .prettierrc
