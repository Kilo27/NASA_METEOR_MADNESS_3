/* global createUnityInstance */
import React, { useEffect, useRef } from "react";
import "./App.css";
function App() {
  const canvasRef = useRef(null);
  const unityInstanceRef = useRef(null); 
  var xpix="1920";
  var ypix="1200";

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;

    const buildUrl = process.env.PUBLIC_URL + "/unity_data/Build";
    const loaderUrl = buildUrl + "/webbuild.loader.js";
    const config = {
      dataUrl: buildUrl + "/webbuild.data",
      frameworkUrl: buildUrl + "/webbuild.framework.js",
      codeUrl: buildUrl + "/webbuild.wasm",
      companyName: "DefaultCompany",
      productName: "Hackathon_my_branch",
      productVersion: "0.1.0",
    };

    const script = document.createElement("script");
    script.src = loaderUrl;
    script.onload = () => {
      if (window.createUnityInstance) {
        window
          .createUnityInstance(canvas, config, (progress) => {
            document.querySelector("#unity-progress-bar-full").style.width =100 * progress + "%";})
          .then((unityInstance) => {
            unityInstanceRef.current = unityInstance;
            document.querySelector("#unity-loading-bar").style.display = "none";
            document.querySelector("#unity-fullscreen-button").onclick = () => {
              unityInstance.SetFullscreen(1);
            };
          })
          .catch((message) => alert(message));
      }
    };
    document.body.appendChild(script);

    return () => {
      if (unityInstanceRef.current) {
        unityInstanceRef.current.Quit();
      }
      document.body.removeChild(script);
    };
  }, []);

  return (
    <div className="App">
      <div id="unity-container" className="unity-desktop">
        <canvas
          ref={canvasRef}
          id="unity-canvas"
          width={xpix}
          height={ypix}
          tabIndex="-1"
        />
        <div id="unity-loading-bar">
          <div id="unity-logo"></div>
          <div id="unity-progress-bar-empty">
            <div id="unity-progress-bar-full"></div>
          </div>
        </div>
        <div id="unity-warning"></div>
        <div id="unity-footer">
          <div id="unity-logo-title-footer"></div>
          <div id="unity-fullscreen-button">Fullscreen</div>
          <div id="unity-build-title">Hackathon_my_branch</div>
        </div>
      </div>
    </div>
  );
}
export default App;