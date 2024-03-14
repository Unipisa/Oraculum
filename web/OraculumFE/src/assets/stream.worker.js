self.addEventListener('message', e => {
  const { apiUrl, requestData, accessToken } = e.data;

  fetch(apiUrl, {
    method: 'POST',
    body: JSON.stringify(requestData),
    headers: {
      'Content-Type': 'application/json',
      ...(accessToken ? { 'Authorization': `Bearer ${accessToken}` } : {})
    }
  }).then(response => {
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    const reader = response.body.getReader();
    let accumulatedText = '';

    function processChunk(chunk) {
      // console.log(chunk);
      accumulatedText += chunk;

      let braceCounter = 0;
      let lastObjectEnd = 0;
      for (let i = 0; i < accumulatedText.length; i++) {
        const char = accumulatedText[i];
        if (char === '{') {
          if (braceCounter === 0) {
            lastObjectEnd = i; // Mark start of a JSON object
          }
          braceCounter++;
        } else if (char === '}') {
          braceCounter--;
          if (braceCounter === 0) {
            // Complete JSON object
            const jsonText = accumulatedText.substring(lastObjectEnd, i + 1);
            try {
              const json = JSON.parse(jsonText);
              self.postMessage(json);
            } catch (error) {
              console.error('Error parsing JSON:', error);
              break; // Exit the loop, wait for more data
            }
            accumulatedText = accumulatedText.substring(i + 1); // Reset accumulatedText
            i = -1; // Reset index
          }
        }
      }
    }

    function read() {
      reader.read().then(({ done, value }) => {
        if (done) {
          // Handle any remaining text when the stream ends
          if (accumulatedText.trim() !== '') {
            try {
              const json = JSON.parse(accumulatedText);
              self.postMessage(json);
            } catch (error) {
              console.error('Error parsing JSON in final chunk:', error);
            }
          }
          self.postMessage('END_OF_STREAM');
          return;
        }

        const chunk = new TextDecoder().decode(value, { stream: true });
        processChunk(chunk);

        read();
      }).catch(error => {
        console.error('Stream reading error:', error);
        self.postMessage('STREAM_READING_ERROR');
      });
    }

    read();
  }).catch(error => {
    console.error('Fetch error:', error);
    self.postMessage('FETCH_ERROR');
  });
});
