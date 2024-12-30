const button = document.querySelector(".menubutton");
const background = document.querySelector(".menu");
let isShowing = true;

button.addEventListener("click", function () {
  if (isShowing) {
    background.style.position = "absolute";
    background.style.display = "flex";
    isShowing = false;
  } else {
    background.style.position = "relative";
    background.style.display = "none";
    isShowing = true;
  }
});

const walletBtn = document.querySelector(".wallet");

walletBtn.addEventListener("click", () => {
  alert("Wallet points coming soon");
});
 