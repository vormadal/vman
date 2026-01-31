#!/usr/bin/env node

/**
 * Smart API Client Generator
 * 
 * This script:
 * 1. Checks if the backend API is running
 * 2. Downloads the OpenAPI spec
 * 3. Compares with cached version to detect changes
 * 4. Generates Kiota client only if spec changed or client missing
 */

const https = require('https');
const http = require('http');
const { execSync } = require('child_process');
const fs = require('fs');
const path = require('path');
const crypto = require('crypto');

// Configuration
const CONFIG = {
  openApiUrl: process.env.NEXT_PUBLIC_API_URL 
    ? `${process.env.NEXT_PUBLIC_API_URL}/openapi/v1.json` 
    : 'http://localhost:5000/openapi/v1.json',
  outputDir: './src/lib/api/generated',
  cacheDir: './.kiota-cache',
  cacheFile: './.kiota-cache/openapi-spec.json',
  hashFile: './.kiota-cache/spec-hash.txt',
  kiotaConfig: './kiota-config.json',
  maxRetries: 3,
  retryDelay: 2000,
};

// Colors for console output
const colors = {
  reset: '\x1b[0m',
  bright: '\x1b[1m',
  red: '\x1b[31m',
  green: '\x1b[32m',
  yellow: '\x1b[33m',
  blue: '\x1b[34m',
  cyan: '\x1b[36m',
};

function log(message, color = colors.reset) {
  console.log(`${color}${message}${colors.reset}`);
}

function error(message) {
  log(`❌ ${message}`, colors.red);
}

function success(message) {
  log(`✅ ${message}`, colors.green);
}

function info(message) {
  log(`ℹ️  ${message}`, colors.cyan);
}

function warning(message) {
  log(`⚠️  ${message}`, colors.yellow);
}

// Ensure cache directory exists
function ensureCacheDir() {
  if (!fs.existsSync(CONFIG.cacheDir)) {
    fs.mkdirSync(CONFIG.cacheDir, { recursive: true });
  }
}

// Download OpenAPI spec with retry logic
async function downloadOpenApiSpec(retries = CONFIG.maxRetries) {
  return new Promise((resolve, reject) => {
    // Determine if we need HTTP or HTTPS
    const isHttps = CONFIG.openApiUrl.startsWith('https://');
    const protocol = isHttps ? https : http;
    
    const options = isHttps 
      ? { 
          agent: new https.Agent({ rejectUnauthorized: false }) // Accept self-signed certificates
        }
      : {};

    protocol.get(CONFIG.openApiUrl, options, (res) => {
      let data = '';

      if (res.statusCode !== 200) {
        reject(new Error(`HTTP ${res.statusCode}: ${res.statusMessage}`));
        return;
      }

      res.on('data', (chunk) => {
        data += chunk;
      });

      res.on('end', () => {
        try {
          const spec = JSON.parse(data);
          resolve(spec);
        } catch (err) {
          reject(new Error(`Failed to parse JSON: ${err.message}`));
        }
      });
    }).on('error', async (err) => {
      if (retries > 1) {
        warning(`Connection failed, retrying... (${CONFIG.maxRetries - retries + 1}/${CONFIG.maxRetries})`);
        await new Promise(r => setTimeout(r, CONFIG.retryDelay));
        try {
          const result = await downloadOpenApiSpec(retries - 1);
          resolve(result);
        } catch (e) {
          reject(e);
        }
      } else {
        reject(err);
      }
    });
  });
}

// Calculate hash of OpenAPI spec
function calculateHash(spec) {
  const content = JSON.stringify(spec, null, 2);
  return crypto.createHash('sha256').update(content).digest('hex');
}

// Check if client needs regeneration
function needsRegeneration(spec) {
  // Check if output directory exists and has files
  if (!fs.existsSync(CONFIG.outputDir)) {
    return { needed: true, reason: 'Client directory does not exist' };
  }

  const files = fs.readdirSync(CONFIG.outputDir);
  if (files.length === 0) {
    return { needed: true, reason: 'Client directory is empty' };
  }

  // Check if hash file exists
  if (!fs.existsSync(CONFIG.hashFile)) {
    return { needed: true, reason: 'No previous spec hash found' };
  }

  // Compare hashes
  const currentHash = calculateHash(spec);
  const previousHash = fs.readFileSync(CONFIG.hashFile, 'utf8').trim();

  if (currentHash !== previousHash) {
    return { needed: true, reason: 'OpenAPI spec has changed' };
  }

  return { needed: false, reason: 'Client is up-to-date' };
}

// Save spec and hash to cache
function saveToCache(spec) {
  ensureCacheDir();
  const content = JSON.stringify(spec, null, 2);
  fs.writeFileSync(CONFIG.cacheFile, content);
  const hash = calculateHash(spec);
  fs.writeFileSync(CONFIG.hashFile, hash);
}

// Generate client using Kiota
function generateClient() {
  try {
    info('Running Kiota client generator...');
    
    // Use direct CLI arguments instead of config file
    const isHttps = CONFIG.openApiUrl.startsWith('https://');
    const args = [
      '--package=@microsoft/kiota',
      'kiota',
      'generate',
      '--openapi', CONFIG.openApiUrl,
      '--language', 'TypeScript',
      '--output', CONFIG.outputDir,
      '--class-name', 'VideoManagerApiClient',
      '--namespace-name', 'VideoManagerApi',
      '--clean-output',
      '--exclude-backward-compatible',
    ];
    
    // Only add SSL validation flag for HTTPS
    if (isHttps) {
      args.push('--disable-ssl-validation');
    }
    
    execSync(`npx ${args.join(' ')}`, {
      stdio: 'inherit',
    });
    return true;
  } catch (err) {
    error(`Kiota generation failed: ${err.message}`);
    return false;
  }
}

// Check if Kiota is available
function checkKiota() {
  try {
    execSync('npx --package=@microsoft/kiota kiota --version', { stdio: 'pipe' });
    return true;
  } catch (err) {
    return false;
  }
}

// Main execution
async function main() {
  log('\n🚀 Smart API Client Generator\n', colors.bright);

  // Check if backend is running
  info('Checking backend API availability...');
  let spec;
  try {
    spec = await downloadOpenApiSpec();
    success(`Backend API is running (OpenAPI ${spec.openapi})`);
    info(`API: ${spec.info.title} v${spec.info.version}`);
  } catch (err) {
    error(`Backend API is not reachable: ${err.message}`);
    warning('Make sure the backend is running:');
    console.log('  cd VideoManager\\VideoManager.AppHost');
    console.log('  aspire run\n');
    process.exit(1);
  }

  // Check if spec has paths
  const pathCount = Object.keys(spec.paths || {}).length;
  if (pathCount === 0) {
    warning('OpenAPI spec has no endpoints defined yet');
    info('Endpoints will appear after implementing controllers');
  } else {
    success(`Found ${pathCount} endpoint(s) in OpenAPI spec`);
  }

  // Check if regeneration is needed
  const { needed, reason } = needsRegeneration(spec);
  
  if (!needed) {
    success(reason);
    log('\n✨ No action needed - client is already up-to-date\n', colors.green);
    return;
  }

  info(reason);

  // Check Kiota availability
  if (!checkKiota()) {
    warning('Kiota not found, installing @microsoft/kiota@1.29.0...');
    try {
      execSync('npm install --save-dev @microsoft/kiota@1.29.0', { stdio: 'inherit' });
    } catch (err) {
      error('Failed to install Kiota');
      process.exit(1);
    }
  }

  // Generate client
  log('\n📦 Generating TypeScript client...\n', colors.blue);
  if (generateClient()) {
    saveToCache(spec);
    success('Client generated successfully!');
    log(`\n✨ Generated client available at: ${colors.cyan}${CONFIG.outputDir}${colors.reset}\n`);
  } else {
    error('Client generation failed');
    process.exit(1);
  }
}

// Run if executed directly
if (require.main === module) {
  main().catch((err) => {
    error(`Unexpected error: ${err.message}`);
    process.exit(1);
  });
}

module.exports = { main, downloadOpenApiSpec, needsRegeneration };
