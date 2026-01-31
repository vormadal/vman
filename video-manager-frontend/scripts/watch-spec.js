#!/usr/bin/env node

/**
 * OpenAPI Spec Watcher
 * 
 * Continuously monitors the backend OpenAPI spec and regenerates
 * the client when changes are detected.
 */

const { downloadOpenApiSpec, needsRegeneration } = require('./generate-client.js');
const { execSync } = require('child_process');

const POLL_INTERVAL = 5000; // Check every 5 seconds

const colors = {
  reset: '\x1b[0m',
  cyan: '\x1b[36m',
  yellow: '\x1b[33m',
  green: '\x1b[32m',
  red: '\x1b[31m',
};

function log(message, color = colors.reset) {
  const timestamp = new Date().toLocaleTimeString();
  console.log(`${color}[${timestamp}] ${message}${colors.reset}`);
}

let isGenerating = false;
let backendAvailable = false;

async function checkAndGenerate() {
  if (isGenerating) {
    return;
  }

  try {
    const spec = await downloadOpenApiSpec(1); // Only 1 retry for watching
    
    if (!backendAvailable) {
      log('✅ Backend API is now available', colors.green);
      backendAvailable = true;
    }

    const { needed, reason } = needsRegeneration(spec);
    
    if (needed) {
      log(`🔄 ${reason} - Regenerating client...`, colors.yellow);
      isGenerating = true;
      
      try {
        execSync('node scripts/generate-client.js', { stdio: 'inherit' });
        log('✅ Client regenerated successfully', colors.green);
      } catch (err) {
        log('❌ Client regeneration failed', colors.red);
      } finally {
        isGenerating = false;
      }
    }
  } catch (err) {
    if (backendAvailable) {
      log('⚠️  Backend API is no longer reachable', colors.yellow);
      backendAvailable = false;
    }
  }
}

async function watch() {
  log('👀 Watching OpenAPI spec for changes...', colors.cyan);
  log('Press Ctrl+C to stop\n', colors.cyan);

  // Initial check
  await checkAndGenerate();

  // Poll for changes
  setInterval(checkAndGenerate, POLL_INTERVAL);
}

watch().catch((err) => {
  log(`Unexpected error: ${err.message}`, colors.red);
  process.exit(1);
});
