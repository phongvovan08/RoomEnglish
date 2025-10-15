// Test trực tiếp OpenAI TTS API
async function testOpenAIDirectly() {
  const apiKey = 'sk-proj-xxxx'
  
  console.log('🧪 Testing OpenAI API directly...')
  
  try {
    const response = await fetch('https://api.openai.com/v1/audio/speech', {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${apiKey}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        model: 'tts-1',
        input: 'Hello! This is a test of OpenAI TTS from RoomEnglish application.',
        voice: 'alloy'
      })
    })
    
    console.log('📡 Response:', {
      status: response.status,
      statusText: response.statusText,
      headers: Object.fromEntries(response.headers.entries())
    })
    
    if (response.ok) {
      const audioBuffer = await response.arrayBuffer()
      console.log('✅ Success! Audio buffer size:', audioBuffer.byteLength, 'bytes')
      
      // Test play audio
      const blob = new Blob([audioBuffer], { type: 'audio/mp3' })
      const audioUrl = URL.createObjectURL(blob)
      const audio = new Audio(audioUrl)
      
      audio.onloadeddata = () => {
        console.log('🎵 Audio loaded, duration:', audio.duration, 'seconds')
      }
      
      audio.onplay = () => {
        console.log('▶️ Playing audio...')
      }
      
      audio.onended = () => {
        console.log('⏹️ Audio finished')
        URL.revokeObjectURL(audioUrl)
      }
      
      audio.onerror = (e) => {
        console.error('❌ Audio play error:', e)
      }
      
      await audio.play()
      return true
      
    } else {
      const errorText = await response.text()
      console.error('❌ API Error:', {
        status: response.status,
        statusText: response.statusText,
        body: errorText
      })
      
      // Parse error if JSON
      try {
        const errorJson = JSON.parse(errorText)
        console.error('📋 Error details:', errorJson)
      } catch (e) {
        console.log('📝 Error text:', errorText)
      }
      
      return false
    }
    
  } catch (error) {
    console.error('❌ Network/Fetch Error:', error)
    return false
  }
}

// Auto-run test
console.log('🚀 Starting OpenAI API test...')
testOpenAIDirectly().then(result => {
  console.log('🏁 Test completed:', result ? 'SUCCESS' : 'FAILED')
}).catch(error => {
  console.error('💥 Test crashed:', error)
})