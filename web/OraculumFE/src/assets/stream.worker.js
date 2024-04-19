self.addEventListener('message', e => {
  const { apiUrl, requestData, accessToken } = e.data;

  fetch(apiUrl, {
    method: 'POST',
    body: JSON.stringify(requestData),
    headers: {
      'Content-Type': 'application/json',
      ...(accessToken ? { 'Authorization': `Bearer ${accessToken}` } : {})
    }
  })
  .then(response => {
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    return response.body.getReader();
  })
  .then(reader => {
    let readerClosed = false;
    let accumulatedText = '';

    function processChunk(text) {
      accumulatedText += text;
      let braceLevel = 0;
      let lastChunkStart = 0;

      for (let i = 0; i < accumulatedText.length; i++) {
        switch (accumulatedText[i]) {
          case '{':
            braceLevel++;
            if (braceLevel === 1) {
              lastChunkStart = i;
            }
            break;
          case '}':
            braceLevel--;
            if (braceLevel === 0) {
              const jsonText = accumulatedText.slice(lastChunkStart, i + 1);
              try {
                const jsonObj = JSON.parse(jsonText);
                self.postMessage(jsonObj);
              } catch (error) {
                self.postMessage({ error: `Error parsing JSON: ${error.message}` });
                // Possible malformed JSON, attempt to continue parsing next objects
              }
              accumulatedText = accumulatedText.slice(i + 1);
              i = lastChunkStart - 1;
            }
            break;
        }
      }

      if (braceLevel !== 0 && readerClosed) {
        // Handle case where the stream has ended but JSON is incomplete
        self.postMessage({ error: 'Stream ended but JSON is incomplete.' });
      }
    }

    function read() {
      reader.read().then(({ done, value }) => {
        if (done) {
          readerClosed = true;
          // Process any remaining text which could be a complete JSON object
          if (accumulatedText.trim() !== '') {
            processChunk('');
          }
          self.postMessage('END_OF_STREAM');
          return;
        }
        const chunk = new TextDecoder().decode(value);
        processChunk(chunk);
        read();
      }).catch(error => {
        self.postMessage({ error: `Stream reading error: ${error.message}` });
      });
    }

    read();
  })
  .catch(error => {
    self.postMessage({ error: `Fetch error: ${error.message}` });
  });
});
