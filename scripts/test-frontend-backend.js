/**
 * Teste simples para verificar comunicação frontend-backend
 */

const axios = require('axios');

const BACKEND_URL = 'http://localhost:8000';

async function testBackendConnection() {
  console.log('🧪 TESTE DE COMUNICAÇÃO FRONTEND-BACKEND');
  console.log('==================================================\n');

  try {
    // 1. Teste Health Check
    console.log('1. Testando Health Check...');
    const healthResponse = await axios.get(`${BACKEND_URL}/health`);
    console.log(`✅ Health Check: ${healthResponse.data.status}`);
    console.log(`   Service: ${healthResponse.data.service}`);
    console.log(`   Version: ${healthResponse.data.version}\n`);

    // 2. Teste Agente
    console.log('2. Testando comunicação com o Agente...');
    const agentResponse = await axios.post(`${BACKEND_URL}/agent/message`, {
      message: 'Olá! Você está funcionando?'
    }, { timeout: 10000 });
    
    console.log(`✅ Agente respondeu: ${JSON.stringify(agentResponse.data)}`);
    console.log(`   Resposta completa: ${agentResponse.data.response || 'N/A'}`);
    console.log(`   Modelo: ${agentResponse.data.model_used || 'N/A'}`);
    console.log(`   Provider: ${agentResponse.data.provider || 'N/A'}\n`);

    // 3. Teste Screenshot
    console.log('3. Testando endpoint de Screenshot...');
    const screenshotResponse = await axios.post(`${BACKEND_URL}/system/screenshot`);
    console.log(`✅ Screenshot: ${screenshotResponse.data.message}\n`);

    // 4. Teste Toggle Orb
    console.log('4. Testando endpoint de Toggle Orb...');
    const toggleResponse = await axios.post(`${BACKEND_URL}/system/toggle-orb`);
    console.log(`✅ Toggle Orb: ${toggleResponse.data.message}\n`);

    console.log('🎉 TODOS OS TESTES PASSARAM!');
    console.log('==================================================');
    console.log('✅ Backend está funcionando corretamente');
    console.log('✅ Frontend pode se comunicar com o backend');
    console.log('✅ Todos os endpoints estão respondendo');
    console.log('\n📋 PRÓXIMOS PASSOS:');
    console.log('1. Execute: npm run start (para iniciar frontend + backend)');
    console.log('2. Ou: npm run dev (para desenvolvimento com hot-reload)');
    console.log('3. O ORB estará disponível como aplicação desktop');

  } catch (error) {
    console.error('\n❌ TESTE FALHOU');
    console.error('==================================================');
    console.error(`Erro: ${error.message}`);
    
    if (error.code === 'ECONNREFUSED') {
      console.error('\n🔧 SOLUÇÃO:');
      console.error('1. Certifique-se de que o backend está rodando:');
      console.error('   cd backend && python main.py');
      console.error('2. Ou execute: npm run start');
    } else if (error.response) {
      console.error(`\nStatus HTTP: ${error.response.status}`);
      console.error(`Resposta: ${JSON.stringify(error.response.data)}`);
    }
    
    process.exit(1);
  }
}

// Executar teste
testBackendConnection();
