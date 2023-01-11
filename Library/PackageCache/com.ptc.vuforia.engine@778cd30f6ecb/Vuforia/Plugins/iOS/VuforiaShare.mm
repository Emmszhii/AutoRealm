/*===============================================================================
Copyright (c) 2022 PTC Inc. All Rights Reserved.

Confidential and Proprietary - Protected under copyright and other laws.
Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.
===============================================================================*/

#import "Foundation/Foundation.h"
#import "UIKit/UIKit.h"

@interface VuforiaSharePopup : NSObject

@property (readonly) UIActivityViewController* activity;
@property (readonly) UIViewController* rootViewController;

- (instancetype) initWithActivity:(UIActivityViewController *) activity
                        presenter:(UIViewController *) rootViewController;

- (void) presentPopup;

@end

@implementation VuforiaSharePopup

- (instancetype) initWithActivity:(UIActivityViewController *)activity presenter:(UIViewController *)rootViewController {
    self = [super init];
    
    if (self) {
        _activity = activity;
        _rootViewController = rootViewController;
    }
    
    return self;
}

- (void) presentPopup {
        
    // On iPad the share popups are usually shown in the center of the screen
    // On iPhone they are automatically anchored to the bottom
    if (UIDevice.currentDevice.userInterfaceIdiom == UIUserInterfaceIdiomPad) {
        
        _activity.popoverPresentationController.sourceView = _rootViewController.view;
        _activity.popoverPresentationController.permittedArrowDirections = UIPopoverArrowDirection(0);
        [self centerPopup];
        
        // We need to keep alive the reference to this class beyond the lifetime of this function.
        // By using selfID in setCompletionWithItemsHandler, self is not deallocated until the popup is closed.
        // This allows us to listen to the OrientationDidChange event while the popup stays open.
        id selfId = self;
        NSNotificationCenter *notificationCenter = NSNotificationCenter.defaultCenter;

        // Observing changes in orientation to re-center the popup on iPad
        [notificationCenter addObserver:selfId selector:@selector(onOrientationDidChange) name:UIDeviceOrientationDidChangeNotification object:UIDevice.currentDevice];

        [_activity setCompletionWithItemsHandler:^(UIActivityType type, BOOL completed, NSArray *items, NSError *error) {
            // Sharing was completed or the popup closed, so we unsubscribe from the NotificationCenter
            [notificationCenter removeObserver:selfId];
        }];
    }
    
    [_rootViewController presentViewController:_activity animated:true completion:nil];
}

- (void) onOrientationDidChange {

    [self centerPopup];
}

- (void) centerPopup {
    
    _activity.popoverPresentationController.sourceRect = CGRectMake(UIScreen.mainScreen.bounds.size.width / 2, UIScreen.mainScreen.bounds.size.height / 2, 0, 0);
}

@end

extern "C"
{
    bool VuforiaShare_Share(const char* filePath) {
        
        if (filePath == nil && strlen(filePath) == 0) {
            NSLog(@"ERROR: The provided path is empty.");
            return false;
            
        }
        
        NSURL *fileURL = [NSURL fileURLWithPath:[NSString stringWithUTF8String:filePath] isDirectory:FALSE];
        if (!fileURL.isFileURL) {
            NSLog(@"ERROR: The provided string is not a valid file path.");
            return false;
        }
        
        if (![NSFileManager.defaultManager fileExistsAtPath:fileURL.path]) {
            NSLog(@"ERROR: The specified file does not exist.");
            return false;
        }
        
        NSArray *filesToShare = @[fileURL];
        
        UIActivityViewController *activityViewController = [[UIActivityViewController alloc] initWithActivityItems:filesToShare applicationActivities:nil];
        UIViewController *viewController = [[[[UIApplication sharedApplication] delegate] window] rootViewController];
        
        VuforiaSharePopup *popup = [[VuforiaSharePopup alloc] initWithActivity:activityViewController presenter:viewController];
        [popup presentPopup];
        
        return true;
    }
}
