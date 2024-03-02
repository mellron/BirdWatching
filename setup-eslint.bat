@echo off
REM Install eslint
call npm install eslint --save-dev

REM Install eslint-plugin-react
REM call npm install eslint-plugin-react --save-dev

REM Initialize eslint
call npx eslint --init

REM Install eslint-config-prettier
call npm install --save-dev eslint-config-prettier

REM Install prettier with exact version
call npm install --save-dev --save-exact prettier

REM Create .prettierrc file with empty JSON object
echo {} > .prettierrc
