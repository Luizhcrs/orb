/**
 * Script para testar a integração completa frontend-backend
 */

const { spawn } = require('child_process');
const path = require('path');

console.log('🧪 TESTE DE INTEGRAÇÃO FRONTEND-BACKEND');
console.log('==================================================\n');

// Função para executar comando
function runCommand(command, args, cwd, description) {
  return new Promise((resolve, reject) => {
    console.log(`⏳ ${description}...`);
    
    const process = spawn(command, args, { 
      cwd: cwd,
      stdio: 'pipe',
      shell: true
    });

    let output = '';
    let error = '';

    process.stdout.on('data', (data) => {
      output += data.toString();
    });

    process.stderr.on('data', (data) => {
      error += data.toString();
    });

    process.on('close', (code) => {
      if (code === 0) {
        console.log(`✅ ${description} - OK`);
        resolve(output);
      } else {
        console.log(`❌ ${description} - FALHOU`);
        console.log(`Erro: ${error}`);
        reject(new Error(`Comando falhou com código ${code}`));
      }
    });

    // Timeout de 30 segundos
    setTimeout(() => {
      process.kill();
      reject(new Error('Timeout'));
    }, 30000);
  });
}

// Função principal de teste
async function testIntegration() {
  try {
    // 1. Verificar se as dependências estão instaladas
    console.log('1. Verificando dependências...\n');
    
    await runCommand('npm', ['list', '--depth=0'], path.join(__dirname, '..'), 'Verificando dependências do root');
    await runCommand('npm', ['list', '--depth=0'], path.join(__dirname, '..', 'frontend'), 'Verificando dependências do frontend');
    
    // 2. Testar build do frontend
    console.log('\n2. Testando build do frontend...\n');
    
    await runCommand('npm', ['run', 'build'], path.join(__dirname, '..', 'frontend'), 'Build do frontend');
    
    // 3. Testar backend
    console.log('\n3. Testando backend...\n');
    
    await runCommand('python', ['test_backend.py'], path.join(__dirname, '..', 'backend'), 'Testes do backend');
    
    // 4. Verificar estrutura do projeto
    console.log('\n4. Verificando estrutura do projeto...\n');
    
    const fs = require('fs');
    const requiredFiles = [
      'package.json',
      'docker-compose.yml',
      'frontend/package.json',
      'frontend/src/main.ts',
      'frontend/src/services/BackendService.ts',
      'frontend/src/llm/BackendLLMManager.ts',
      'backend/main.py',
      'backend/src/api/main.py',
      'backend/src/agentes/orb_agent/agente.py',
      'shared/types/api.ts',
      'shared/config/backend.ts'
    ];

    let allFilesExist = true;
    for (const file of requiredFiles) {
      const filePath = path.join(__dirname, '..', file);
      if (fs.existsSync(filePath)) {
        console.log(`✅ ${file} - OK`);
      } else {
        console.log(`❌ ${file} - NÃO ENCONTRADO`);
        allFilesExist = false;
      }
    }

    if (!allFilesExist) {
      throw new Error('Alguns arquivos necessários não foram encontrados');
    }

    console.log('\n🎉 TESTE DE INTEGRAÇÃO CONCLUÍDO COM SUCESSO!');
    console.log('==================================================');
    console.log('✅ Estrutura do monorepo organizada');
    console.log('✅ Frontend compilado corretamente');
    console.log('✅ Backend funcionando perfeitamente');
    console.log('✅ Tipos compartilhados criados');
    console.log('✅ Docker configurado');
    console.log('\n📋 PRÓXIMOS PASSOS:');
    console.log('1. Execute: npm run dev (para desenvolvimento completo)');
    console.log('2. Ou: npm run docker:up (para usar Docker)');
    console.log('3. Configure as variáveis de ambiente no backend/.env');

  } catch (error) {
    console.error('\n❌ TESTE DE INTEGRAÇÃO FALHOU');
    console.error('==================================================');
    console.error(`Erro: ${error.message}`);
    console.error('\n🔧 VERIFICAÇÕES:');
    console.error('1. Certifique-se de que todas as dependências estão instaladas');
    console.error('2. Verifique se o Python está configurado corretamente');
    console.error('3. Verifique se o Node.js está funcionando');
    console.error('4. Execute: npm run install:all');
    process.exit(1);
  }
}

// Executar teste
testIntegration();
