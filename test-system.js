/**
 * Comprehensive System Test Suite
 * Tests all critical endpoints and functionality
 */

const API_BASE = 'https://api.taktiq.app/api';
const results = {
  passed: [],
  failed: [],
  performance: []
};

// Test helper
async function testEndpoint(name, url, options = {}) {
  const startTime = Date.now();
  try {
    const response = await fetch(url, options);
    const endTime = Date.now();
    const duration = endTime - startTime;

    const status = response.status;
    const success = options.expectedStatus ? status === options.expectedStatus : (status >= 200 && status < 300);

    const result = {
      name,
      url,
      status,
      duration: `${duration}ms`,
      success
    };

    if (success) {
      results.passed.push(result);
      console.log(`✅ ${name} - ${status} (${duration}ms)`);
    } else {
      results.failed.push(result);
      console.log(`❌ ${name} - ${status} (${duration}ms)`);
    }

    results.performance.push({ name, duration });
    return result;
  } catch (error) {
    const result = {
      name,
      url,
      error: error.message,
      success: false
    };
    results.failed.push(result);
    console.log(`❌ ${name} - ERROR: ${error.message}`);
    return result;
  }
}

async function runTests() {
  console.log('🧪 Starting Comprehensive System Tests...\n');
  console.log('📊 Testing Public Endpoints (No Auth)\n');

  // Public endpoints
  await testEndpoint('Get All Public Trainers', `${API_BASE}/trainer`);
  await testEndpoint('Get All Published Posts', `${API_BASE}/posts?page=1&pageSize=10`);

  console.log('\n📊 Testing Authentication Endpoints\n');

  // Auth endpoints (should return 401 without auth)
  await testEndpoint('Get Current User (No Auth)', `${API_BASE}/me`, { expectedStatus: 401 });

  console.log('\n📊 Performance Analysis\n');

  // Calculate average response times
  const avgResponseTime = results.performance.reduce((sum, p) => sum + p.duration, 0) / results.performance.length;
  console.log(`Average Response Time: ${avgResponseTime.toFixed(2)}ms`);

  // Find slow endpoints (> 500ms)
  const slowEndpoints = results.performance.filter(p => p.duration > 500);
  if (slowEndpoints.length > 0) {
    console.log('\n⚠️  Slow Endpoints (>500ms):');
    slowEndpoints.forEach(ep => {
      console.log(`   - ${ep.name}: ${ep.duration}ms`);
    });
  }

  // Find very slow endpoints (> 1000ms)
  const verySlowEndpoints = results.performance.filter(p => p.duration > 1000);
  if (verySlowEndpoints.length > 0) {
    console.log('\n🔴 Very Slow Endpoints (>1s):');
    verySlowEndpoints.forEach(ep => {
      console.log(`   - ${ep.name}: ${ep.duration}ms`);
    });
  }

  console.log('\n📈 Test Summary\n');
  console.log(`✅ Passed: ${results.passed.length}`);
  console.log(`❌ Failed: ${results.failed.length}`);
  console.log(`📊 Total Tests: ${results.passed.length + results.failed.length}`);

  if (results.failed.length > 0) {
    console.log('\n❌ Failed Tests:');
    results.failed.forEach(f => {
      console.log(`   - ${f.name}: ${f.error || `Status ${f.status}`}`);
    });
  }

  console.log('\n✨ Tests Complete!\n');

  return {
    totalTests: results.passed.length + results.failed.length,
    passed: results.passed.length,
    failed: results.failed.length,
    avgResponseTime: avgResponseTime.toFixed(2),
    slowEndpoints: slowEndpoints.length,
    verySlowEndpoints: verySlowEndpoints.length
  };
}

// Run the tests
runTests().then(summary => {
  console.log('📋 Final Summary:', summary);
  process.exit(results.failed.length > 0 ? 1 : 0);
}).catch(error => {
  console.error('💥 Test suite error:', error);
  process.exit(1);
});
