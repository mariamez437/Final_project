document
  .getElementById("forgetForm")
  .addEventListener("submit", async function (e) {
    e.preventDefault();

    const email = document.getElementById("email").value.trim();
    const emailError = document.getElementById("emailError");

    emailError.textContent = "";

    if (!email) {
      emailError.textContent = "Email is required";
      emailError.classList.add("text-danger");
      return;
    }

    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailPattern.test(email)) {
      emailError.textContent = "Enter a valid email";
      emailError.classList.add("text-danger");
      return;
    }

    try {
      const response = await fetch(
        "http://localhost:5194/api/Auth/request-reset-code",
        {
          method: "POST",
          body: JSON.stringify({ email }),
          headers: {
            "Content-Type": "application/json",
          },
        }
      );

      if (response.ok) {
        // showAlert("Check your email for reset instructions.", "success");
        ShowBootstrapToast(
          "Check your email for reset instructions.",
          "success"
        );
           setTimeout(() => {
        window.location.replace("resetPassword.html");
      }, 2000)

      } else {
        // showAlert("Email not found or server error.", "danger");
        ShowBootstrapToast("Email not found or server error.", "danger");
      }
    } catch (error) {
      // showAlert("Something went wrong. Try again", "info");
      ("Something went wrong. Try again", "danger");
    }
  });

  
//----------------------------------------------------------------------------------------
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
