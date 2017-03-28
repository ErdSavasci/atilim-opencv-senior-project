%Get All Image Names
allImages = dir('*.bmp');
clc;

%Iterate Over All Images and Flip Them
for i = 1 : length(allImages)
    imageToBeModified = imread(allImages(i).name);
    imageToBeModified = imageToBeModified(:, end:-1:1, :);
    imwrite(imageToBeModified, allImages(i).name);
    clc;
    fprintf('Progress: %d over %d', i, length(allImages));
end
