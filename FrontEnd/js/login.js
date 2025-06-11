document.addEventListener("DOMContentLoaded", function () {
  const loginForm = document.querySelector(".sign-in-form");
  if (loginForm) {
    loginForm.addEventListener("submit", async function (event) {
      event.preventDefault();

      const Email = document.getElementById("email").value.trim();
      const Password = document.getElementById("password").value;
      const emailError = document.getElementById("emailError");
      const passwordError = document.getElementById("passwordError");

      emailError.textContent = "";
      passwordError.textContent = "";
      let isValid = true;
      const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      if (Email == "") {
        emailError.textContent = "Email is required";
        emailError.classList.add("text-danger");
        isValid = false;
      } else if (!emailPattern.test(Email)) {
        emailError.textContent = "It is must be email ";
        isValid = false;
      }

      if (Password === "") {
        passwordError.textContent = "Password is required";
        passwordError.classList.add("text-danger");
        isValid = false;
      }
      if (isValid) {
        try {
          const formData = new FormData();
          formData.append("Email", Email);
          formData.append("Password", Password);

          const response = await fetch(
            "http://localhost:5194/api/Auth/Login",
            {
              method: "POST",
              body: formData,
            }
          );

          if (response.ok) {
            console.log("success");
            var data = await response.json();
            localStorage.setItem("token", data["token"]);
            localStorage.setItem("type", data["type"]);

            // الانتقال إلى صفحة الهوم
            window.location.replace("../../index.html");
          } else {
            const errorData = await response.text();
            ShowBootstrapToast("Login failed:" + errorData, "danger");
            console.log("Login failed:", errorData);
          }
        } catch (error) {
          console.error("Login error:", error);
          ShowBootstrapToast("Login error:" + error, "danger");
          // alert("An error occurred during login.");
        }
      }
    });
  }
});
window.ShowBootstrapToast = function (
  message,
  type = "Info",
  withButtons = false
) {
  const toastId = "custom-toast-" + Date.now();
  const toastHTML = `
        <div id="${toastId}" class="toast align-items-center text-white bg-${type.toLowerCase()} border-0" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body w-100">
                    ${message}
                    ${
                      withButtons
                        ? `
                    <div class="mt-2 pt-2 border-top d-flex justify-content-end gap-2">
                        <button type="button" class="btn btn-light btn-sm" id="btn-add-new">Add Another</button>
                        <button type="button" class="btn btn-outline-light btn-sm" id="btn-go-home">Go Home</button>
                    </div>`
                        : ""
                    }
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>
    `;

  let toastContainer = document.getElementById("toast-container");
  if (!toastContainer) {
    toastContainer = document.createElement("div");
    toastContainer.id = "toast-container";
    toastContainer.className =
      "toast-container position-fixed bottom-0 end-0 p-3";
    document.body.appendChild(toastContainer);
  }

  toastContainer.innerHTML += toastHTML;

  const toastElement = document.getElementById(toastId);
  const toast = new bootstrap.Toast(toastElement, { delay: 7000 });
  toast.show();

  if (withButtons) {
    toastElement
      .querySelector("#btn-add-new")
      .addEventListener("click", function () {
        $("#register-card-2-form")[0].reset();
        $("#preview").attr(
          "src",
          "images/id-card-illustration_23-2147829294.avif"
        );
        $(".error-msg").text("");
        toast.hide();
      });

    toastElement
      .querySelector("#btn-go-home")
      .addEventListener("click", function () {
        window.location.replace("../../index.html");
      });
  }

  toastElement.addEventListener("hidden.bs.toast", function () {
    toastElement.remove();
  });
};
