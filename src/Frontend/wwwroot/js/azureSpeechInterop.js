// azureSpeechInterop.js

window.azureSpeechInterop = {
    region: "uksouth",

    createSpeechSynthesizer: async function () {
        var token = await this.getToken();
        var speechConfig = SpeechSDK.SpeechConfig.fromAuthorizationToken(token, "uksouth");
        speechConfig.speechSynthesisVoiceName = "ur-IN-SalmanNeural";
        var synthesizer = new SpeechSDK.SpeechSynthesizer(speechConfig);

        window.synthesizer = synthesizer;
    },

    getToken: async function () {
        const response = await fetch("https://localhost:7071/api/speech/token", {
            method: "GET",
            headers: {
                "Content-Type": "application/json"
            }
        });

        if (response.ok) {
            const data = await response.json();
            return data.token;
        } else {
            console.error("Failed to get the token:", response);
            return null;
        }
    },

    speak: async function (phrase) {

        window.synthesizer.speakTextAsync(
            phrase,
            function (result) {
                if (result.reason === SpeechSDK.ResultReason.SynthesizingAudioCompleted) {
                    resultDiv.innerHTML += "Synthesis finished for [" + phrase + "].\n";
                } else if (result.reason === SpeechSDK.ResultReason.Canceled) {
                    resultDiv.innerHTML += "Synthesis failed. Error detail: " + result.errorDetails + "\n";
                }
                console.log(result);
                synthesizer.close();
                synthesizer = undefined;

                dotNetObject.invokeMethodAsync("HandleSpeakTextResult", resultDiv.innerHTML);
            },
            function (err) {
                resultDiv.innerHTML += "Error: " + err + "\n";
                console.log(err);
                synthesizer.close();
                synthesizer = undefined;

                dotNetObject.invokeMethodAsync("HandleSpeakTextResult", resultDiv.innerHTML);
            });
    }
};
