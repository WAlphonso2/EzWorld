import sys
import os
sys.path.append(os.path.abspath("Shap_E"))
import torch
from Shap_E.shap_e.diffusion.sample import sample_latents
from Shap_E.shap_e.diffusion.gaussian_diffusion import diffusion_from_config
from Shap_E.shap_e.models.download import load_model, load_config
from Shap_E.shap_e.util.notebooks import decode_latent_mesh

def main():
    device = torch.device('cuda' if torch.cuda.is_available() else 'cpu')
    xm = load_model('transmitter', device=device)
    model = load_model('text300M', device=device)
    diffusion = diffusion_from_config(load_config('diffusion'))
    print("Finished setting up model")
    batch_size = 1
    guidance_scale = 15.0
    prompt = " ".join(sys.argv[2:])
    file_name = sys.argv[1]
    latents = sample_latents(
        batch_size=batch_size,
        model=model,
        diffusion=diffusion,
        guidance_scale=guidance_scale,
        model_kwargs=dict(texts=[prompt] * batch_size),
        progress=True,
        clip_denoised=True,
        use_fp16=True,
        use_karras=True,
        karras_steps=64,
        sigma_min=1e-3,
        sigma_max=160,
        s_churn=0,
    )

    for i, latent in enumerate(latents):
        t = decode_latent_mesh(xm, latent).tri_mesh()
        with open(f'{file_name}.obj', 'w') as f:
            t.write_obj(f)


if __name__ == "__main__":
    main()

