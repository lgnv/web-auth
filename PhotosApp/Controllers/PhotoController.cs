﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PhotosApp.Data;
using PhotosApp.Models;

namespace PhotosApp.Controllers
{
        
    [Authorize]
    public class PhotoController : Controller
    {
        private readonly IPhotoRepository photoRepository;
        private readonly IWebHostEnvironment env;
        private readonly IMapper mapper;

        public PhotoController(IPhotoRepository photoRepository, IWebHostEnvironment env, IMapper mapper)
        {
            this.photoRepository = photoRepository;
            this.env = env;
            this.mapper = mapper;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var ownerId = GetOwnerId();
            var photoEntities = await photoRepository.GetPhotosAsync(ownerId);
            var photos = mapper.Map<IEnumerable<Photo>>(photoEntities);

            var model = new PhotoIndexModel(photos.ToList());
            return View(model);
        }
        [Authorize(Policy = "MustOwnPhoto")]
        public async Task<IActionResult> GetPhoto(Guid id)
        {
            var photoEntity = await photoRepository.GetPhotoAsync(id);
            if (photoEntity == null)
                return NotFound();

            var photo = mapper.Map<Photo>(photoEntity);

            var model = new GetPhotoModel(photo);
            return View(model);
        }

        [Authorize(Policy = "Beta")]
        [Authorize(Policy = "MustOwnPhoto")]
        public async Task<IActionResult> EditPhoto(Guid id)
        {
            var photo = await photoRepository.GetPhotoAsync(id);
            if (photo == null)
                return NotFound();

            var viewModel = new EditPhotoModel
            {
                Id = photo.Id,
                Title = photo.Title
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MustOwnPhoto")]
        public async Task<IActionResult> EditPhoto(EditPhotoModel editPhotoModel)
        {
            if (editPhotoModel == null || !ModelState.IsValid)
                return View();

            var photoEntity = await photoRepository.GetPhotoAsync(editPhotoModel.Id);
            if (photoEntity == null)
                return NotFound();

            mapper.Map(editPhotoModel, photoEntity);

            await photoRepository.UpdatePhotoAsync(photoEntity);
            if (!await photoRepository.SaveAsync())
                throw new Exception($"Updating photo with {editPhotoModel.Id} failed on save.");

            return RedirectToAction("Index");
        }
        
        [Authorize(Policy = "MustOwnPhoto")]
        public async Task<IActionResult> DeletePhoto(Guid id)
        {
            var photoEntity = await photoRepository.GetPhotoAsync(id);
            if (photoEntity == null)
                return NotFound();

            await photoRepository.DeletePhotoAsync(photoEntity);

            if (!await photoRepository.SaveAsync())
                throw new Exception($"Deleting photo with {id} failed on save.");

            return RedirectToAction("Index");
        }

        [Authorize(Policy = "Beta")]
        [Authorize(Policy = "CanAddPhoto")]
        public IActionResult AddPhoto()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Beta")]
        [Authorize(Policy = "CanAddPhoto")]
        public async Task<IActionResult> AddPhoto(AddPhotoModel addPhotoModel)
        {
            if (addPhotoModel == null || !ModelState.IsValid)
                return View();

            var file = addPhotoModel.Files.FirstOrDefault();
            if (file == null || file.Length == 0)
                return View();

            var fileName = SavePhotoFile(file);
            var photoEntity = mapper.Map<PhotoEntity>(addPhotoModel);
            photoEntity.FileName = fileName;
            var ownerId = GetOwnerId();
            photoEntity.OwnerId = ownerId;

            await photoRepository.AddPhotoAsync(photoEntity);
            if (!await photoRepository.SaveAsync())
                throw new Exception($"Adding a photo failed on save.");

            return RedirectToAction("Index");
        }

        private string SavePhotoFile(IFormFile file)
        {
            byte[] photoBytes;
            using (var fileStream = file.OpenReadStream())
            using (var memoryStream = new MemoryStream())
            {
                fileStream.CopyTo(memoryStream);
                photoBytes = memoryStream.ToArray();
            }

            var webRootPath = env.WebRootPath;
            var fileName = Guid.NewGuid() + ".jpg";
            var filePath = Path.Combine($"{webRootPath}/photos/{fileName}");
            System.IO.File.WriteAllBytes(filePath, photoBytes);
            return fileName;
        }

        private string GetOwnerId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
