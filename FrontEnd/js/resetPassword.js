document
  .getElementById("resetForm")
  .addEventListener("submit", async function (e) {
    e.preventDefault();

    const params = new URLSearchParams(window.location.search);
    const token = params.get("token");

    const newPassword = document.getElementById("newPassword").value;
    const confirmPassword = document.getElementById("confirmPassword").value;
    const passwordError = document.getElementById("passwordError");
    const confirmPasswordError = document.getElementById(
      "confirmPasswordError"
    );

    // Reset errors
    passwordError.textContent = "";
    confirmPasswordError.textContent = "";
    passwordError.classList.remove("text-danger");
    confirmPasswordError.classList.remove("text-danger");

    let isValid = true;

    if (!newPassword) {
      passwordError.textContent = "Password is required.";
      passwordError.classList.add("text-danger");
      isValid = false;
    } else if (newPassword.length < 8) {
      passwordError.textContent = "Password must be at least 8 characters.";
      passwordError.classList.add("text-danger");
      isValid = false;
    }

    if (!confirmPassword) {
      confirmPasswordError.textContent = "Confirm Password is required.";
      confirmPasswordError.classList.add("text-danger");
      isValid = false;
    }

    if (!isValid) return;

    if (newPassword !== confirmPassword) {
      confirmPasswordError.textContent = "Passwords do not match.";
      confirmPasswordError.classList.add("text-danger");
      return;
    }

    try {
      const response = await fetch(
        "http://localhost:5194/api/Auth/ResetPassword",
        {
          method: "POST",
          body: JSON.stringify({ token, newPassword }),
          headers: {
            "Content-Type": "application/json",
          },
        }
      );

      if (response.ok) {
        ShowBootstrapToast(
          "Password has been reset. Redirecting to login...",
          "success"
        );
        setTimeout(() => {
          window.location.href = "login.html";
        }, 2000);
      } else {
        ShowBootstrapToast("Failed to reset password", "danger");
      }
    } catch (error) {
      ShowBootstrapToast("Something went wrong.", "danger");
    }
  });

// function showAlert(message, type = "success") {
//   const alertBox = document.getElementById("alertBox");
//   alertBox.textContent = message;
//   alertBox.className = `alert alert-${type} mt-3`; // success, danger, warning, etc.
//   alertBox.classList.remove("d-none");

//   // Auto-hide after 3 seconds
//   setTimeout(() => {
//     alertBox.classList.add("d-none");
//   }, 9000);
// }
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
