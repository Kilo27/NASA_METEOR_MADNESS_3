/* global createUnityInstance */
import React, { useEffect, useRef } from "react";
import "./App.css";

function App() {
  const canvasRef = useRef(null);

  useEffect(() => {
    const canvas = canvasRef.current;

    function unityShowBanner(msg, type) {
      const warningBanner = document.querySelector("#unity-warning");
      function updateBannerVisibility() {
        warningBanner.style.display = warningBanner.children.length ? "block" : "none";
      }
      const div = document.createElement("div");
      div.innerHTML = msg;
      warningBanner.appendChild(div);
      if (type === "error") div.style = "background: red; padding: 10px;";
      else {
        if (type === "warning") div.style = "background: yellow; padding: 10px;";
        setTimeout(() => {
          warningBanner.removeChild(div);
          updateBannerVisibility();
        }, 5000);
      }
      updateBannerVisibility();
    }

    const buildUrl = process.env.PUBLIC_URL + "/unity_data/Build";
    const loaderUrl = buildUrl + "/WebBUild2.loader.js";
    const config = {
      arguments: [],
      dataUrl: buildUrl + "/WebBUild2.data",
      frameworkUrl: buildUrl + "/WebBUild2.framework.js",
      codeUrl: buildUrl + "/WebBUild2.wasm",
      streamingAssetsUrl: "StreamingAssets",
      companyName: "DefaultCompany",
      productName: "Hackathon_my_branch",
      productVersion: "0.1.0",
      showBanner: unityShowBanner,
    };

    // Mobile vs desktop setup
    if (/iPhone|iPad|iPod|Android/i.test(navigator.userAgent)) {
      document.querySelector("#unity-container").className = "unity-mobile";
      canvas.className = "unity-mobile";
    } else {
      canvas.style.width = "1920px";
      canvas.style.height = "1200px";
    }

    document.querySelector("#unity-loading-bar").style.display = "block";

    // Load Unity loader script
    const script = document.createElement("script");
    script.src = loaderUrl;
    script.onload = () => {
      if (window.createUnityInstance) {
        window.createUnityInstance(canvas, config, (progress) => {
          document.querySelector("#unity-progress-bar-full").style.width =
            100 * progress + "%";
        })
          .then((unityInstance) => {
            document.querySelector("#unity-loading-bar").style.display = "none";
            document.querySelector("#unity-fullscreen-button").onclick = () => {
              unityInstance.SetFullscreen(1);
            };
          })
          .catch((message) => {
            alert(message);
          });
      }
    };
    document.body.appendChild(script);

    return () => {
      document.body.removeChild(script);
    };
  }, []);

  return (
    <div className="App">
      <div id="unity-container" className="unity-desktop">
        <canvas ref={canvasRef} id="unity-canvas" width="1920" height="1200" tabIndex="-1"></canvas>
        <div id="unity-loading-bar">
          <div id="unity-logo"></div>
          <div id="unity-progress-bar-empty">
            <div id="unity-progress-bar-full"></div>
          </div>
        </div>
        <div id="unity-warning"></div>
        <div id="unity-footer">
          <div id="unity-logo-title-footer"></div>
          <div id="unity-fullscreen-button"></div>
          <div id="unity-build-title">Hackathon_my_branch</div>
        </div>
      </div>
    </div>
  );
}

export default App;