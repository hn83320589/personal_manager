/**
 * 前端測試執行腳本
 * 自動執行單元測試和 E2E 測試，並生成測試報告
 */

import { spawn } from 'child_process'
import fs from 'fs'
import path from 'path'

const PROJECT_ROOT = process.cwd()
const TEST_RESULTS_DIR = path.join(PROJECT_ROOT, 'test-results')

// 確保測試結果目錄存在
if (!fs.existsSync(TEST_RESULTS_DIR)) {
  fs.mkdirSync(TEST_RESULTS_DIR, { recursive: true })
}

/**
 * 執行命令並收集輸出
 */
function runCommand(command, args = [], options = {}) {
  return new Promise((resolve, reject) => {
    console.log(`\n🔄 執行命令: ${command} ${args.join(' ')}`)
    
    const child = spawn(command, args, {
      stdio: 'pipe',
      shell: true,
      ...options
    })
    
    let stdout = ''
    let stderr = ''
    
    child.stdout.on('data', (data) => {
      const output = data.toString()
      stdout += output
      process.stdout.write(output)
    })
    
    child.stderr.on('data', (data) => {
      const output = data.toString()
      stderr += output
      process.stderr.write(output)
    })
    
    child.on('close', (code) => {
      resolve({ code, stdout, stderr })
    })
    
    child.on('error', (error) => {
      reject(error)
    })
  })
}

/**
 * 生成測試報告
 */
function generateTestReport(results) {
  const timestamp = new Date().toISOString()
  const report = {
    timestamp,
    summary: {
      total: 0,
      passed: 0,
      failed: 0,
      skipped: 0
    },
    tests: results
  }
  
  // 計算總計
  results.forEach(test => {
    if (test.result && test.result.stdout) {
      const output = test.result.stdout
      
      // 簡單的測試結果解析
      const passedMatch = output.match(/(\d+) passed/)
      const failedMatch = output.match(/(\d+) failed/)
      const skippedMatch = output.match(/(\d+) skipped/)
      
      if (passedMatch) report.summary.passed += parseInt(passedMatch[1])
      if (failedMatch) report.summary.failed += parseInt(failedMatch[1])
      if (skippedMatch) report.summary.skipped += parseInt(skippedMatch[1])
    }
  })
  
  report.summary.total = report.summary.passed + report.summary.failed + report.summary.skipped
  
  // 保存報告
  const reportPath = path.join(TEST_RESULTS_DIR, `test-report-${Date.now()}.json`)
  fs.writeFileSync(reportPath, JSON.stringify(report, null, 2))
  
  // 生成 HTML 報告
  generateHtmlReport(report)
  
  return report
}

/**
 * 生成 HTML 測試報告
 */
function generateHtmlReport(report) {
  const html = `
<!DOCTYPE html>
<html lang="zh-TW">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Personal Manager Frontend - 測試報告</title>
    <style>
        body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; margin: 2rem; }
        .header { background: #f8fafc; padding: 2rem; border-radius: 8px; margin-bottom: 2rem; }
        .summary { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 1rem; margin: 2rem 0; }
        .metric { background: white; padding: 1.5rem; border-radius: 8px; box-shadow: 0 1px 3px rgba(0,0,0,0.1); text-align: center; }
        .metric-value { font-size: 2rem; font-weight: bold; }
        .passed { color: #10b981; }
        .failed { color: #ef4444; }
        .skipped { color: #f59e0b; }
        .total { color: #6366f1; }
        .test-section { margin: 2rem 0; }
        .test-item { background: white; margin: 1rem 0; padding: 1rem; border-radius: 8px; box-shadow: 0 1px 3px rgba(0,0,0,0.1); }
        .success { border-left: 4px solid #10b981; }
        .error { border-left: 4px solid #ef4444; }
        .code { background: #f1f5f9; padding: 1rem; border-radius: 4px; overflow-x: auto; font-family: 'Monaco', 'Consolas', monospace; }
        .timestamp { color: #64748b; font-size: 0.875rem; }
    </style>
</head>
<body>
    <div class="header">
        <h1>Personal Manager Frontend 測試報告</h1>
        <p class="timestamp">生成時間: ${new Date(report.timestamp).toLocaleString('zh-TW')}</p>
    </div>
    
    <div class="summary">
        <div class="metric">
            <div class="metric-value total">${report.summary.total}</div>
            <div>總測試數</div>
        </div>
        <div class="metric">
            <div class="metric-value passed">${report.summary.passed}</div>
            <div>通過</div>
        </div>
        <div class="metric">
            <div class="metric-value failed">${report.summary.failed}</div>
            <div>失敗</div>
        </div>
        <div class="metric">
            <div class="metric-value skipped">${report.summary.skipped}</div>
            <div>跳過</div>
        </div>
    </div>
    
    <div class="test-section">
        <h2>測試詳情</h2>
        ${report.tests.map(test => `
            <div class="test-item ${test.result.code === 0 ? 'success' : 'error'}">
                <h3>${test.name}</h3>
                <p><strong>狀態:</strong> ${test.result.code === 0 ? '✅ 成功' : '❌ 失敗'}</p>
                <p><strong>結束代碼:</strong> ${test.result.code}</p>
                ${test.result.stderr ? `
                    <h4>錯誤輸出:</h4>
                    <div class="code">${test.result.stderr.replace(/</g, '&lt;').replace(/>/g, '&gt;')}</div>
                ` : ''}
                ${test.result.stdout ? `
                    <h4>標準輸出:</h4>
                    <div class="code">${test.result.stdout.replace(/</g, '&lt;').replace(/>/g, '&gt;')}</div>
                ` : ''}
            </div>
        `).join('')}
    </div>
</body>
</html>
  `
  
  const htmlPath = path.join(TEST_RESULTS_DIR, 'test-report.html')
  fs.writeFileSync(htmlPath, html)
  console.log(`\n📊 HTML 測試報告已生成: ${htmlPath}`)
}

/**
 * 主要測試執行流程
 */
async function runTests() {
  console.log('🚀 開始執行 Personal Manager Frontend 測試套件')
  console.log('=' .repeat(60))
  
  const results = []
  
  try {
    // 1. 執行單元測試
    console.log('\n📋 步驟 1: 執行單元測試')
    const unitTestResult = await runCommand('npm', ['run', 'test:unit', '--', '--run'])
    results.push({
      name: '單元測試 (Vitest)',
      type: 'unit',
      result: unitTestResult
    })
    
    // 2. 執行類型檢查
    console.log('\n📋 步驟 2: 執行 TypeScript 類型檢查')
    const typeCheckResult = await runCommand('npm', ['run', 'type-check'])
    results.push({
      name: 'TypeScript 類型檢查',
      type: 'typecheck',
      result: typeCheckResult
    })
    
    // 3. 執行建置測試
    console.log('\n📋 步驟 3: 執行前端建置')
    const buildResult = await runCommand('npm', ['run', 'build'])
    results.push({
      name: '前端建置測試',
      type: 'build',
      result: buildResult
    })
    
    // 4. 如果有 E2E 測試設定，執行 E2E 測試
    try {
      console.log('\n📋 步驟 4: 執行 E2E 測試 (如果可用)')
      const e2eResult = await runCommand('npm', ['run', 'test:e2e'])
      results.push({
        name: 'E2E 測試 (Playwright)',
        type: 'e2e',
        result: e2eResult
      })
    } catch (error) {
      console.log('⚠️  E2E 測試暫時跳過 (可能需要額外設定)')
    }
    
  } catch (error) {
    console.error('❌ 測試執行過程中發生錯誤:', error)
  }
  
  // 生成測試報告
  console.log('\n📊 生成測試報告...')
  const report = generateTestReport(results)
  
  // 輸出摘要
  console.log('\n' + '=' .repeat(60))
  console.log('📈 測試摘要')
  console.log('=' .repeat(60))
  console.log(`總測試數: ${report.summary.total}`)
  console.log(`✅ 通過: ${report.summary.passed}`)
  console.log(`❌ 失敗: ${report.summary.failed}`)
  console.log(`⏭️  跳過: ${report.summary.skipped}`)
  
  // 測試狀態摘要
  results.forEach(test => {
    const status = test.result.code === 0 ? '✅' : '❌'
    console.log(`${status} ${test.name}: ${test.result.code === 0 ? '成功' : '失敗'}`)
  })
  
  console.log('\n🏁 測試執行完成!')
  
  // 如果有失敗的測試，以非零狀態碼結束
  const hasFailures = results.some(test => test.result.code !== 0)
  if (hasFailures) {
    process.exit(1)
  }
}

// 執行測試
runTests().catch(error => {
  console.error('💥 測試執行器發生錯誤:', error)
  process.exit(1)
})